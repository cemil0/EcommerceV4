namespace ECommerce.Application.Exceptions;

/// <summary>
/// Exception thrown when order business rules are violated
/// </summary>
public class OrderBusinessRuleException : Exception
{
    public string ErrorCode { get; }
    
    public OrderBusinessRuleException(string message, string errorCode) 
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
