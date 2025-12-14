namespace ECommerce.Application.DTOs.Admin;

// ==================== PRODUCT DTOs ====================

public class AdminProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int TotalStock { get; set; }
    public bool IsActive { get; set; }
    public int VariantCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminProductDetailDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
    
    // New product detail fields
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? ShortDescription { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNewArrival { get; set; }
    
    public List<AdminVariantDto> Variants { get; set; } = new();
    public List<ProductImageDto> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class AdminVariantDto
{
    public int VariantId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}

// ==================== PRODUCT REQUESTS ====================

public class CreateProductRequest
{
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal BasePrice { get; set; }

    // Expanded properties
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    
    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    
    // Flags
    public bool IsFeatured { get; set; }
    public bool IsNewArrival { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateProductRequest
{
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal BasePrice { get; set; }
    
    // Expanded properties
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    
    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    
    // Flags
    public bool IsFeatured { get; set; }
    public bool IsNewArrival { get; set; }
    public bool IsActive { get; set; }
}

public class CreateVariantRequest
{
    public string VariantName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

public class UpdateVariantRequest
{
    public string VariantName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateStockRequest
{
    public int Quantity { get; set; }
}

public class BulkStockUpdateRequest
{
    public List<StockUpdateItem> Items { get; set; } = new();
}

public class StockUpdateItem
{
    public int VariantId { get; set; }
    public int Quantity { get; set; }
}

// ==================== ORDER DTOs ====================

public class AdminOrderDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public decimal TotalAmount { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminOrderDetailDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public CustomerSummary Customer { get; set; } = new();
    public CompanySummary? Company { get; set; }
    public List<OrderItemSummary> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public AddressSummary ShippingAddress { get; set; } = new();
    public AddressSummary BillingAddress { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CustomerSummary
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class CompanySummary
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
}

public class OrderItemSummary
{
    public int OrderItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class AddressSummary
{
    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}

public class OrderTimelineDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public List<OrderTimelineEvent> Events { get; set; } = new();
}

public class OrderTimelineEvent
{
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? PerformedBy { get; set; }
}

// ==================== ORDER REQUESTS ====================

public class OrderStatusUpdateRequest
{
    public string NewStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class RefundRequest
{
    public decimal RefundAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class OrderFilterRequest : Common.PagedRequest
{
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? CustomerEmail { get; set; }
}

public class OrderStatisticsDto
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
}
