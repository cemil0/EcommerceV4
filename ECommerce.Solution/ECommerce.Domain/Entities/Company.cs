namespace ECommerce.Domain.Entities;

public class Company
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string TaxOffice { get; set; } = string.Empty;
    public string? CompanyType { get; set; }
    public string? Industry { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // B2B Properties
    public decimal CreditLimit { get; set; } = 0;
    public decimal CurrentBalance { get; set; } = 0;
    public int PaymentTermDays { get; set; } = 30;
    public bool IsApprovalRequired { get; set; } = false;
    public int? PriceListId { get; set; }
    public decimal DiscountPercentage { get; set; } = 0;

    // Navigation properties
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual B2BPriceList? PriceList { get; set; }
    public virtual ICollection<CompanyCampaign> Campaigns { get; set; } = new List<CompanyCampaign>();
    public virtual ICollection<CompanyApprovalRule> ApprovalRules { get; set; } = new List<CompanyApprovalRule>();
}
