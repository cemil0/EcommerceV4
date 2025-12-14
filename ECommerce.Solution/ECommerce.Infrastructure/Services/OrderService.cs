using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Exceptions;
using ECommerce.Infrastructure.Extensions;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStockReservationService _stockReservationService;
    private readonly IPriceValidationService _priceValidationService;
    private readonly IOrderBusinessRules _orderBusinessRules;
    private readonly IOrderStateMachine _orderStateMachine;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        IStockReservationService stockReservationService,
        IPriceValidationService priceValidationService,
        IOrderBusinessRules orderBusinessRules,
        IOrderStateMachine orderStateMachine,
        ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _stockReservationService = stockReservationService;
        _priceValidationService = priceValidationService;
        _orderBusinessRules = orderBusinessRules;
        _orderStateMachine = orderStateMachine;
        _logger = logger;
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id);
        return order != null ? _mapper.Map<OrderDto>(order) : null;
    }

    public async Task<OrderDto?> GetByOrderNumberAsync(string orderNumber)
    {
        var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber);
        return order != null ? _mapper.Map<OrderDto>(order) : null;
    }

    public async Task<IEnumerable<OrderDto>> GetByCustomerIdAsync(int customerId)
    {
        var orders = await _unitOfWork.Orders.GetByCustomerIdAsync(customerId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        var orders = await _unitOfWork.Orders.GetAllAsync();
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto> CreateB2COrderAsync(CreateOrderRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 0. LOAD CART WITH ITEMS
            var cart = await _unitOfWork.Carts.GetActiveCartWithItemsAsync(request.CustomerId);
            if (cart == null || !cart.CartItems.Any())
            {
                throw new InvalidOperationException("Cart is empty or not found");
            }

            // If Items not provided, populate from cart
            if (!request.Items.Any())
            {
                request.Items = cart.CartItems.Select(ci => new CreateOrderItemRequest
                {
                    ProductVariantId = ci.ProductVariantId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.ProductVariant?.SalePrice ?? ci.ProductVariant?.BasePrice ?? 0
                }).ToList();
            }

            // 1. BUSINESS RULES VALIDATION
            var businessRulesResult = await _orderBusinessRules.ValidateB2COrderAsync(request);
            if (!businessRulesResult.IsValid)
            {
                throw new OrderBusinessRuleException(
                    businessRulesResult.ErrorMessage!,
                    businessRulesResult.ErrorCode!);
            }

            // 2. STOCK RESERVATION
            var stockItems = request.Items.Select(i => new StockReservationItemDto
            {
                ProductVariantId = i.ProductVariantId,
                Quantity = i.Quantity
            }).ToList();

            var stockResult = await _stockReservationService.ReserveStockAsync(stockItems);
            
            if (!stockResult.IsSuccess)
            {
                throw new StockNotAvailableException(stockResult.ErrorMessage!);
            }

            // 3. PRICE VALIDATION
            var priceItems = request.Items.Select(i => new PriceValidationItemDto
            {
                ProductVariantId = i.ProductVariantId,
                ExpectedPrice = i.UnitPrice
            }).ToList();

            var priceResult = await _priceValidationService.ValidatePricesAsync(priceItems);
            
            if (!priceResult.IsValid)
            {
                throw new PriceChangedException(priceResult.PriceChanges);
            }

            // 4. Generate order number
            var orderNumber = await GenerateOrderNumberAsync();

            // 5. Calculate totals and create order items
            decimal subtotal = 0;
            decimal totalTax = 0;
            var orderItems = new List<OrderItem>();

            // Load variants with products
            var variantIds = request.Items.Select(i => i.ProductVariantId).ToList();
            var variants = await _unitOfWork.ProductVariants
                .Query()
                .Include(v => v.Product)
                .Where(v => variantIds.Contains(v.ProductVariantId))
                .ToListAsync();

            foreach (var item in request.Items)
            {
                var variant = variants.FirstOrDefault(v => v.ProductVariantId == item.ProductVariantId);
                if (variant == null)
                    throw new InvalidOperationException($"Product variant {item.ProductVariantId} not found");

                var unitPrice = item.UnitPrice;
                var itemSubtotal = unitPrice * item.Quantity;
                var itemTax = itemSubtotal * 0.20m; // 20% KDV
                
                subtotal += itemSubtotal;
                totalTax += itemTax;

                orderItems.Add(new OrderItem
                {
                    ProductVariantId = item.ProductVariantId,
                    ProductName = variant.Product?.ProductName ?? string.Empty,
                    VariantName = variant.VariantName,
                    SKU = variant.VariantSKU,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    DiscountAmount = 0,
                    TaxAmount = itemTax,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            var taxAmount = totalTax;
            var shippingAmount = 50m; // Fixed shipping
            var totalAmount = subtotal + taxAmount + shippingAmount;

            // 6. Create order
            var order = new Order
            {
                OrderNumber = orderNumber,
                CustomerId = request.CustomerId,
                CompanyId = null,
                OrderType = OrderType.B2C,
                OrderStatus = OrderStatus.Pending,
                SubtotalAmount = subtotal,
                DiscountAmount = 0,
                TaxAmount = taxAmount,
                ShippingAmount = shippingAmount,
                TotalAmount = totalAmount,
                Currency = "TRY",
                BillingAddressId = request.BillingAddressId,
                ShippingAddressId = request.ShippingAddressId,
                CouponCode = request.CouponCode,
                CustomerNotes = request.CustomerNotes,
                OrderDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // 7. Add order items
            foreach (var item in orderItems)
            {
                item.OrderId = order.OrderId;
            }
            await _unitOfWork.OrderItems.AddRangeAsync(orderItems);
            await _unitOfWork.SaveChangesAsync();

            // 8. CLEAR CART after successful order
            _unitOfWork.CartItems.RemoveRange(cart.CartItems);
            _unitOfWork.Carts.Remove(cart);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();

            // 9. Return created order with items
            var createdOrder = await _unitOfWork.Orders
                .Query()
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .Include(o => o.BillingAddress)
                .Include(o => o.ShippingAddress)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            return _mapper.Map<OrderDto>(createdOrder!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "B2C order creation failed for customer {CustomerId}", request.CustomerId);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<OrderDto> CreateB2BOrderAsync(CreateOrderRequest request)
    {
        if (!request.CompanyId.HasValue)
            throw new InvalidOperationException("CompanyId is required for B2B orders");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 0. BUSINESS RULES VALIDATION
            var businessRulesResult = await _orderBusinessRules.ValidateB2BOrderAsync(request, request.CompanyId.Value);
            if (!businessRulesResult.IsValid)
            {
                throw new OrderBusinessRuleException(
                    businessRulesResult.ErrorMessage!,
                    businessRulesResult.ErrorCode!);
            }

            // 1. STOCK RESERVATION
            var stockItems = request.Items.Select(i => new StockReservationItemDto
            {
                ProductVariantId = i.ProductVariantId,
                Quantity = i.Quantity
            }).ToList();

            var stockResult = await _stockReservationService.ReserveStockAsync(stockItems);
            
            if (!stockResult.IsSuccess)
            {
                throw new StockNotAvailableException(stockResult.ErrorMessage!);
            }

            // 2. PRICE VALIDATION
            var priceItems = request.Items.Select(i => new PriceValidationItemDto
            {
                ProductVariantId = i.ProductVariantId,
                ExpectedPrice = i.UnitPrice
            }).ToList();

            var priceResult = await _priceValidationService.ValidatePricesAsync(priceItems);
            
            if (!priceResult.IsValid)
            {
                throw new PriceChangedException(priceResult.PriceChanges);
            }

            // 3. Generate order number
            var orderNumber = await GenerateOrderNumberAsync();

            // 4. Calculate totals
            decimal subtotal = 0;
            decimal totalTax = 0;
            var orderItems = new List<OrderItem>();

            // FIX #1: Use Query + Include
            var variantIds = request.Items.Select(i => i.ProductVariantId).ToList();
            var variants = await _unitOfWork.ProductVariants
                .Query()
                .Include(v => v.Product)
                .Where(v => variantIds.Contains(v.ProductVariantId))
                .ToListAsync();

            foreach (var item in request.Items)
            {
                var variant = variants.FirstOrDefault(v => v.ProductVariantId == item.ProductVariantId);
                if (variant == null)
                    throw new InvalidOperationException($"Product variant {item.ProductVariantId} not found");

                // FIX #4: Use validated price
                var unitPrice = item.UnitPrice;
                var itemSubtotal = unitPrice * item.Quantity;
                var itemTax = itemSubtotal * 0.20m;
                
                subtotal += itemSubtotal;
                totalTax += itemTax; // FIX #3

                orderItems.Add(new OrderItem
                {
                    ProductVariantId = item.ProductVariantId,
                    ProductName = variant.Product?.ProductName ?? string.Empty, // FIX #2: Null-safe
                    VariantName = variant.VariantName,
                    SKU = variant.VariantSKU,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    DiscountAmount = 0,
                    TaxAmount = itemTax,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            var taxAmount = totalTax;
            var shippingAmount = 0m;
            var totalAmount = subtotal + taxAmount + shippingAmount;

            var order = new Order
            {
                OrderNumber = orderNumber,
                CustomerId = request.CustomerId,
                CompanyId = request.CompanyId,
                OrderType = OrderType.B2B,
                OrderStatus = OrderStatus.Approved,
                SubtotalAmount = subtotal,
                DiscountAmount = 0,
                TaxAmount = taxAmount,
                ShippingAmount = shippingAmount,
                TotalAmount = totalAmount,
                Currency = "TRY",
                BillingAddressId = request.BillingAddressId,
                ShippingAddressId = request.ShippingAddressId,
                CustomerNotes = request.CustomerNotes,
                OrderDate = DateTime.UtcNow,
                ApprovedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            foreach (var item in orderItems)
            {
                item.OrderId = order.OrderId;
            }
            await _unitOfWork.OrderItems.AddRangeAsync(orderItems);
            
            // FIX #2: SaveChanges BEFORE CommitTransaction
            await _unitOfWork.SaveChangesAsync();

            // FIX #4: Use state machine for B2B auto-approval to create audit trail
            await _orderStateMachine.TransitionAsync(order.OrderId, OrderStatus.Approved, "B2B order auto-approved");

            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation(
                "B2B order {OrderNumber} created and auto-approved for company {CompanyId}. Total: {Total} TRY",
                orderNumber, request.CompanyId, totalAmount);

            var createdOrder = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber);
            return _mapper.Map<OrderDto>(createdOrder!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "B2B order creation failed for company {CompanyId}", request.CompanyId);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<string> GenerateOrderNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await _unitOfWork.Orders.GetCountForYearAsync(year);
        var orderNumber = $"ORD-{year}-{(count + 1):D6}";
        return orderNumber;
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? reason = null)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            await _orderStateMachine.TransitionAsync(orderId, newStatus, reason);
            await _unitOfWork.CommitTransactionAsync();

            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            return _mapper.Map<OrderDto>(order!);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<List<OrderStatus>> GetValidNextStatesAsync(int orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        return _orderStateMachine.GetValidNextStates(order.OrderStatus, order.OrderType);
    }

    public async Task<PagedResponse<OrderDto>> GetPagedAsync(PagedRequest request)
    {
        var query = _unitOfWork.Orders
            .Query()
            .ApplySorting(request.SortBy ?? "OrderDate", request.SortDescending);

        var pagedOrders = await query.ToPagedResponseAsync(request);
        
        var orderDtos = _mapper.Map<List<OrderDto>>(pagedOrders.Data);
        
        return new PagedResponse<OrderDto>(
            orderDtos,
            pagedOrders.Page,
            pagedOrders.PageSize,
            pagedOrders.TotalCount);
    }

    public async Task<PagedResponse<OrderDto>> GetPagedByCustomerAsync(int customerId, PagedRequest request)
    {
        var query = _unitOfWork.Orders
            .Query()
            .Where(o => o.CustomerId == customerId)
            .ApplySorting(request.SortBy ?? "OrderDate", request.SortDescending);

        var pagedOrders = await query.ToPagedResponseAsync(request);
        
        var orderDtos = _mapper.Map<List<OrderDto>>(pagedOrders.Data);
        
        return new PagedResponse<OrderDto>(
            orderDtos,
            pagedOrders.Page,
            pagedOrders.PageSize,
            pagedOrders.TotalCount);
    }
}
