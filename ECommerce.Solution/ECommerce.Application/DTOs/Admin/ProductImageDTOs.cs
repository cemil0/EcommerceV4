namespace ECommerce.Application.DTOs.Admin;

public class ProductImageDto
{
    public int ImageId { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public string? AltText { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UploadImageRequest
{
    public string? AltText { get; set; }
    public bool SetAsPrimary { get; set; } = false;
}

public class ReorderImagesRequest
{
    public List<ImageOrderItem> Images { get; set; } = new();
}

public class ImageOrderItem
{
    public int ImageId { get; set; }
    public int DisplayOrder { get; set; }
}
