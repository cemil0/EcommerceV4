namespace ECommerce.Application.Exceptions;

/// <summary>
/// Exception thrown when stock is not available for reservation
/// </summary>
public class StockNotAvailableException : Exception
{
    public string ErrorCode => "STOCK_1001";
    
    public StockNotAvailableException(string message) : base(message)
    {
    }
    
    public StockNotAvailableException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
