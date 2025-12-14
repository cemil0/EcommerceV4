using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Order
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int? CompanyId { get; set; }
    public OrderType OrderType { get; set; }
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public int BillingAddressId { get; set; }
    public int ShippingAddressId { get; set; }
    public string? CouponCode { get; set; }
    public string? CustomerNotes { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public DateTime? CancelledDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // B2B Properties
    public int? PaymentTermDays { get; set; }
    public DateTime? DueDate { get; set; }
    public string ApprovalStatus { get; set; } = "Approved";
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? CorporateInvoiceNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? CompanyTaxNumber { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? CostCenter { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public Company? Company { get; set; }
    public Address BillingAddress { get; set; } = null!;
    public Address ShippingAddress { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
