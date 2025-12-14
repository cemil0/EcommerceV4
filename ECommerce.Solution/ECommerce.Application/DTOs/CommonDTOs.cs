namespace ECommerce.Application.DTOs;

public class ProductDto
{
    public int ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Brand { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public CategoryDto? Category { get; set; }
    public List<ProductVariantDto> ProductVariants { get; set; } = new();
}

public class ProductVariantDto
{
    public int ProductVariantId { get; set; }
    public string VariantSKU { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string Currency { get; set; } = "TRY";
    public bool IsActive { get; set; }
    public int StockQuantity { get; set; }
}

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
    public List<CategoryDto> SubCategories { get; set; } = new();
}

public class CartDto
{
    public int CartId { get; set; }
    public int? CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<CartItemDto> CartItems { get; set; } = new();
}

public class CartItemDto
{
    public int CartItemId { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class OrderDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public string? CustomerNotes { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new();
}

public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
