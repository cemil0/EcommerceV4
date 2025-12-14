using ECommerce.Domain.Enums;

namespace ECommerce.Application.Exceptions;

/// <summary>
/// Exception thrown when an invalid order state transition is attempted
/// </summary>
public class InvalidStateTransitionException : Exception
{
    public string ErrorCode => "ORDER_3001";
    public OrderStatus FromState { get; }
    public OrderStatus ToState { get; }
    
    public InvalidStateTransitionException(
        OrderStatus fromState, 
        OrderStatus toState, 
        string? reason = null) 
        : base(BuildMessage(fromState, toState, reason))
    {
        FromState = fromState;
        ToState = toState;
    }
    
    private static string BuildMessage(OrderStatus from, OrderStatus to, string? reason)
    {
        var message = $"Invalid state transition from {from} to {to}.";
        if (!string.IsNullOrEmpty(reason))
        {
            message += $" Reason: {reason}";
        }
        return message;
    }
}
