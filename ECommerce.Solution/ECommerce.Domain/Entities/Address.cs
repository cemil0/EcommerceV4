namespace ECommerce.Domain.Entities;

public class Address
{
    public int AddressId { get; set; }
    public int CustomerId { get; set; }
    public string AddressType { get; set; } = string.Empty; // Billing, Shipping
    public string? AddressTitle { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "Türkiye";
    public bool IsDefault { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Customer Customer { get; set; } = null!;
}
