using ECommerce.Application.DTOs;

namespace ECommerce.Application.Exceptions;

/// <summary>
/// Exception thrown when product prices have changed since cart creation
/// </summary>
public class PriceChangedException : Exception
{
    public string ErrorCode => "PRICE_2001";
    public List<PriceChangeDetail> PriceChanges { get; }
    
    public PriceChangedException(List<PriceChangeDetail> priceChanges) 
        : base(BuildMessage(priceChanges))
    {
        PriceChanges = priceChanges;
    }
    
    private static string BuildMessage(List<PriceChangeDetail> changes)
    {
        if (changes.Count == 1)
        {
            var change = changes[0];
            return $"Price changed for {change.ProductName}. " +
                   $"Expected: {change.ExpectedPrice:C}, Current: {change.CurrentPrice:C}";
        }
        
        return $"{changes.Count} product prices have changed. Please review your cart.";
    }
}
