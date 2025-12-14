namespace ECommerce.Application.DTOs;

/// <summary>
/// DTO for stock reservation - used by Infrastructure layer
/// Avoids layer violation (Infrastructure depending on API DTOs)
/// </summary>
public class StockReservationItemDto
{
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; }
}
