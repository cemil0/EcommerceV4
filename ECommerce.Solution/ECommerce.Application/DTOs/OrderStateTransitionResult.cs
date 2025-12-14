namespace ECommerce.Application.DTOs;

/// <summary>
/// Result of order state transition validation/execution
/// </summary>
public class OrderStateTransitionResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    
    public static OrderStateTransitionResult Success()
    {
        return new OrderStateTransitionResult { IsValid = true };
    }
    
    public static OrderStateTransitionResult Failed(string errorMessage)
    {
        return new OrderStateTransitionResult 
        { 
            IsValid = false, 
            ErrorMessage = errorMessage 
        };
    }
}
