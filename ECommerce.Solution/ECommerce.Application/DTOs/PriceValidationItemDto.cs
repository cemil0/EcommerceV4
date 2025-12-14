namespace ECommerce.Application.DTOs;

/// <summary>
/// DTO for price validation - contains expected price from cart
/// </summary>
public class PriceValidationItemDto
{
    public int ProductVariantId { get; set; }
    public decimal ExpectedPrice { get; set; }
}
