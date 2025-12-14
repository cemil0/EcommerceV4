namespace ECommerce.Domain.Entities;

public class Product
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public bool IsVariantProduct { get; set; } = true;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public bool IsNewArrival { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Category Category { get; set; } = null!;
    public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
