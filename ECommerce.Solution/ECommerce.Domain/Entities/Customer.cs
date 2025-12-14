using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Customer
{
    public int CustomerId { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
    public int? CompanyId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser? ApplicationUser { get; set; }
    public Company? Company { get; set; }
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
