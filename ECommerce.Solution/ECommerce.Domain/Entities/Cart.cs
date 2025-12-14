namespace ECommerce.Domain.Entities;

public class Cart
{
    public int CartId { get; set; }
    public int? CustomerId { get; set; }
    public string? SessionId { get; set; }
    public decimal TotalAmount => CartItems?.Sum(ci => ci.TotalPrice) ?? 0;
    public int TotalItems => CartItems?.Sum(ci => ci.Quantity) ?? 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties
    public Customer? Customer { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
