namespace ECommerce.Domain.Entities;

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty; // Snapshot
    public string VariantName { get; set; } = string.Empty; // Snapshot
    public string SKU { get; set; } = string.Empty; // Snapshot
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; }
    public decimal TotalPrice { get; private set; }  // EF Core will set this via computed column
    public bool IsStockReserved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Order Order { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}
