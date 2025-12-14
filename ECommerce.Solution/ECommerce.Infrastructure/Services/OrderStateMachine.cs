using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Order state machine - manages order lifecycle with proper state transitions
/// CORRECTED: Separate B2C/B2B transitions, stock release on cancel, proper Delivered flow
/// </summary>
public class OrderStateMachine : IOrderStateMachine
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStockReservationService _stockReservationService;
    private readonly ILogger<OrderStateMachine> _logger;

    // B2C Valid Transitions (Pending → Processing → Shipped → Delivered → Returned)
    private static readonly Dictionary<OrderStatus, List<OrderStatus>> ValidTransitionsB2C = new()
    {
        { OrderStatus.Pending, new() { OrderStatus.Processing, OrderStatus.Cancelled } },
        { OrderStatus.Processing, new() { OrderStatus.Shipped, OrderStatus.Cancelled } },
        { OrderStatus.Shipped, new() { OrderStatus.Delivered } },
        { OrderStatus.Delivered, new() { OrderStatus.Returned } },
        { OrderStatus.Cancelled, new() }, // Terminal state
        { OrderStatus.Returned, new() }   // Terminal state
    };

    // B2B Valid Transitions (Approved → Processing → Shipped → Delivered → Returned)
    private static readonly Dictionary<OrderStatus, List<OrderStatus>> ValidTransitionsB2B = new()
    {
        { OrderStatus.Approved, new() { OrderStatus.Processing, OrderStatus.Cancelled } },
        { OrderStatus.Processing, new() { OrderStatus.Shipped, OrderStatus.Cancelled } },
        { OrderStatus.Shipped, new() { OrderStatus.Delivered } },
        { OrderStatus.Delivered, new() { OrderStatus.Returned } },
        { OrderStatus.Cancelled, new() }, // Terminal state
        { OrderStatus.Returned, new() }   // Terminal state
    };

    public OrderStateMachine(
        IUnitOfWork unitOfWork,
        IStockReservationService stockReservationService,
        ILogger<OrderStateMachine> logger)
    {
        _unitOfWork = unitOfWork;
        _stockReservationService = stockReservationService;
        _logger = logger;
    }

    public async Task<OrderStateTransitionResult> ValidateTransitionAsync(
        OrderStatus fromState, 
        OrderStatus toState,
        OrderType orderType)
    {
        // Get correct transition map based on order type
        var validTransitions = orderType == OrderType.B2C 
            ? ValidTransitionsB2C 
            : ValidTransitionsB2B;

        // Check if from state exists in map
        if (!validTransitions.ContainsKey(fromState))
        {
            return OrderStateTransitionResult.Failed(
                $"Invalid state: {fromState} for {orderType} orders");
        }

        // Check if transition is allowed
        if (!validTransitions[fromState].Contains(toState))
        {
            return OrderStateTransitionResult.Failed(
                $"Transition from {fromState} to {toState} is not allowed for {orderType} orders");
        }

        return OrderStateTransitionResult.Success();
    }

    public async Task<bool> TransitionAsync(
        int orderId,
        OrderStatus toState,
        string? reason = null,
        int? userId = null)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        var fromState = order.OrderStatus;

        // Validate transition
        var validationResult = await ValidateTransitionAsync(
            fromState, 
            toState, 
            order.OrderType);

        if (!validationResult.IsValid)
        {
            throw new InvalidStateTransitionException(
                fromState, 
                toState, 
                validationResult.ErrorMessage);
        }

        _logger.LogInformation(
            "Transitioning order {OrderId} from {FromState} to {ToState}. Reason: {Reason}",
            orderId, fromState, toState, reason ?? "None");

        // Update order state
        order.OrderStatus = toState;
        order.UpdatedAt = DateTime.UtcNow;

        // Execute state-specific actions
        await ExecuteStateActionsAsync(order, toState);

        // Create audit trail entry
        var statusHistory = new OrderStatusHistory
        {
            OrderId = orderId,
            FromStatus = fromState,
            ToStatus = toState,
            Reason = reason,
            ChangedByUserId = userId,
            ChangedAt = DateTime.UtcNow
        };
        await _unitOfWork.OrderStatusHistory.AddAsync(statusHistory);

        // Save changes
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Order {OrderId} successfully transitioned from {FromState} to {ToState}",
            orderId, fromState, toState);

        return true;
    }

    public List<OrderStatus> GetValidNextStates(OrderStatus currentState, OrderType orderType)
    {
        var validTransitions = orderType == OrderType.B2C 
            ? ValidTransitionsB2C 
            : ValidTransitionsB2B;

        if (!validTransitions.ContainsKey(currentState))
        {
            return new List<OrderStatus>();
        }

        return validTransitions[currentState];
    }

    private async Task ExecuteStateActionsAsync(Order order, OrderStatus newState)
    {
        switch (newState)
        {
            case OrderStatus.Processing:
                order.ProcessedDate = DateTime.UtcNow;
                _logger.LogInformation("Order {OrderId} processing started", order.OrderId);
                // TODO: Send processing notification
                break;

            case OrderStatus.Shipped:
                order.ShippedDate = DateTime.UtcNow;
                _logger.LogInformation("Order {OrderId} shipped", order.OrderId);
                // TODO: Send shipping notification with tracking
                break;

            case OrderStatus.Delivered:
                order.DeliveredDate = DateTime.UtcNow;
                _logger.LogInformation("Order {OrderId} delivered", order.OrderId);
                // TODO: Send delivery confirmation
                // TODO: Start review request timer
                break;

            case OrderStatus.Cancelled:
                order.CancelledDate = DateTime.UtcNow;
                _logger.LogInformation("Order {OrderId} cancelled", order.OrderId);
                
                // CRITICAL: Release stock reservation
                await ReleaseStockForOrderAsync(order);
                
                // TODO: Initiate refund if payment was made
                break;

            case OrderStatus.Returned:
                _logger.LogInformation("Order {OrderId} returned", order.OrderId);
                // TODO: Process return and refund
                break;
        }
    }

    private async Task ReleaseStockForOrderAsync(Order order)
    {
        _logger.LogInformation("Releasing stock for cancelled order {OrderId}", order.OrderId);

        // Get order items
        var orderItems = await _unitOfWork.OrderItems.GetByOrderIdAsync(order.OrderId);

        foreach (var item in orderItems)
        {
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(item.ProductVariantId);
            
            if (variant == null)
            {
                _logger.LogWarning(
                    "Product variant {VariantId} not found during stock release for order {OrderId}",
                    item.ProductVariantId, order.OrderId);
                continue;
            }

            // Decrease reserved quantity
            if (variant.ReservedQuantity >= item.Quantity)
            {
                variant.ReservedQuantity -= item.Quantity;
                variant.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Released {Quantity} units of variant {VariantId}. Reserved: {Reserved}, Available: {Available}",
                    item.Quantity, variant.ProductVariantId, 
                    variant.ReservedQuantity, variant.StockQuantity - variant.ReservedQuantity);
            }
            else
            {
                _logger.LogWarning(
                    "Reserved quantity mismatch for variant {VariantId}. Reserved: {Reserved}, Requested: {Requested}",
                    variant.ProductVariantId, variant.ReservedQuantity, item.Quantity);
                
                // Set to 0 to prevent negative values
                variant.ReservedQuantity = 0;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Stock released successfully for order {OrderId}", order.OrderId);
    }
}
