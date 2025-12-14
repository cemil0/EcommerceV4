namespace ECommerce.Application.DTOs;

public class CompanyDto
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateCompanyRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
}
