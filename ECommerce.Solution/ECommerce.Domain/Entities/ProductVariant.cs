namespace ECommerce.Domain.Entities;

public class ProductVariant
{
    public int ProductVariantId { get; set; }
    public int ProductId { get; set; }
    public string VariantSKU { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Storage { get; set; }
    public string? RAM { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string? Barcode { get; set; }
    public string? EAN { get; set; }
    
    // Stock Management
    public int StockQuantity { get; set; } = 0;
    public int ReservedQuantity { get; set; } = 0;
    public int AvailableQuantity => StockQuantity - ReservedQuantity;
    
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Product Product { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
