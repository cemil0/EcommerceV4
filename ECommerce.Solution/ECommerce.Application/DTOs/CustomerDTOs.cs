using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs;

public class CustomerDto
{
    public int CustomerId { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
    public int? CompanyId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
}

public class UpdateCustomerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
