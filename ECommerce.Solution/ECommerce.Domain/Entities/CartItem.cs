namespace ECommerce.Domain.Entities;

public class CartItem
{
    public int CartItemId { get; set; }
    public int CartId { get; set; }
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; private set; }  // EF Core will set this via computed column
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Cart Cart { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}
