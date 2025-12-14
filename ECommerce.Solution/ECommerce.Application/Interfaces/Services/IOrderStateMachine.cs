using ECommerce.Application.DTOs;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Interfaces.Services;

/// <summary>
/// Service for managing order state transitions
/// </summary>
public interface IOrderStateMachine
{
    /// <summary>
    /// Validates if a state transition is allowed
    /// </summary>
    Task<OrderStateTransitionResult> ValidateTransitionAsync(
        OrderStatus fromState, 
        OrderStatus toState,
        OrderType orderType);
    
    /// <summary>
    /// Executes a state transition with business rules and state-specific actions
    /// </summary>
    Task<bool> TransitionAsync(
        int orderId,
        OrderStatus toState,
        string? reason = null,
        int? userId = null);
    
    /// <summary>
    /// Gets all valid next states for current state based on order type
    /// </summary>
    List<OrderStatus> GetValidNextStates(OrderStatus currentState, OrderType orderType);
}
