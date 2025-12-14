namespace ECommerce.Application.DTOs;

// B2B Dashboard DTOs
public class B2BDashboardDto
{
    public CompanyFinancialSummary Financial { get; set; } = new();
    public OrderStatistics Orders { get; set; } = new();
}

public class CompanyFinancialSummary
{
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal AvailableCredit { get; set; }
    public decimal CreditUsagePercentage { get; set; }
    public decimal TotalSpent { get; set; }
}

public class OrderStatistics
{
    public int TotalOrders { get; set; }
    public int PendingApproval { get; set; }
    public int Approved { get; set; }
    public decimal AverageOrderValue { get; set; }
}

// B2B Pricing DTOs
public class B2BPriceDto
{
    public int ProductVariantId { get; set; }
    public decimal B2BPrice { get; set; }
    public decimal RetailPrice { get; set; }
    public decimal Savings { get; set; }
}

// Approval DTOs
public class ApprovalRequirement
{
    public bool Required { get; set; }
    public string? ApproverRole { get; set; }
    public int ApprovalLevel { get; set; }
    public string? RuleName { get; set; }
}
