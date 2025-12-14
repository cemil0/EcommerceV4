namespace ECommerce.Application.DTOs;

public class StockReservationResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public List<StockReservationItem> ReservedItems { get; set; } = new();

    public static StockReservationResult Success(List<StockReservationItem> items)
    {
        return new StockReservationResult
        {
            IsSuccess = true,
            ReservedItems = items
        };
    }

    public static StockReservationResult Failed(string errorMessage)
    {
        return new StockReservationResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}

public class StockReservationItem
{
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantityReserved { get; set; }
    public DateTime ReservedAt { get; set; }
}
