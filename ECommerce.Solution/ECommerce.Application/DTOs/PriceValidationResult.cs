namespace ECommerce.Application.DTOs;

/// <summary>
/// Result of price validation operation
/// </summary>
public class PriceValidationResult
{
    public bool IsValid { get; set; }
    public List<PriceChangeDetail> PriceChanges { get; set; } = new();
    
    public static PriceValidationResult Valid()
    {
        return new PriceValidationResult { IsValid = true };
    }
    
    public static PriceValidationResult Invalid(List<PriceChangeDetail> changes)
    {
        return new PriceValidationResult 
        { 
            IsValid = false, 
            PriceChanges = changes 
        };
    }
}

/// <summary>
/// Details of a price change
/// </summary>
public class PriceChangeDetail
{
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ExpectedPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal PriceDifference => CurrentPrice - ExpectedPrice;
    public decimal PercentageChange => ExpectedPrice > 0 
        ? ((CurrentPrice - ExpectedPrice) / ExpectedPrice) * 100 
        : 0;
}
