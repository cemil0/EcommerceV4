namespace ECommerce.Domain.Entities;

public class ProductImage
{
    public int ImageId { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public string? AltText { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Product Product { get; set; } = null!;
}
