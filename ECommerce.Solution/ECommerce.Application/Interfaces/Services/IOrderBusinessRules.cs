using ECommerce.Application.DTOs;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Interfaces.Services;

/// <summary>
/// Business rules validation for B2C and B2B orders
/// </summary>
public interface IOrderBusinessRules
{
    /// <summary>
    /// Validates B2C order creation rules
    /// </summary>
    Task<OrderValidationResult> ValidateB2COrderAsync(CreateOrderRequest request);
    
    /// <summary>
    /// Validates B2B order creation rules
    /// </summary>
    Task<OrderValidationResult> ValidateB2BOrderAsync(CreateOrderRequest request, int companyId);
}

/// <summary>
/// Result of order validation
/// </summary>
public class OrderValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    
    public static OrderValidationResult Success()
    {
        return new OrderValidationResult { IsValid = true };
    }
    
    public static OrderValidationResult Failed(string errorMessage, string errorCode)
    {
        return new OrderValidationResult 
        { 
            IsValid = false, 
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
    }
}
