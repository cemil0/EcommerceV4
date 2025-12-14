using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Business rules validation for orders
/// </summary>
public class OrderBusinessRules : IOrderBusinessRules
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderBusinessRules> _logger;

    // Business rule constants
    private const int B2C_MAX_ITEMS = 10; // BR-009: Max 10 items per B2C order
    private const decimal B2B_MIN_ORDER_AMOUNT = 1000m; // BR-010: Min 1000 TRY for B2B

    public OrderBusinessRules(
        IUnitOfWork unitOfWork,
        ILogger<OrderBusinessRules> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<OrderValidationResult> ValidateB2COrderAsync(CreateOrderRequest request)
    {
        _logger.LogInformation("Validating B2C order for customer {CustomerId}", request.CustomerId);

        // BR-009: Max 10 items per order
        if (request.Items.Count > B2C_MAX_ITEMS)
        {
            _logger.LogWarning(
                "B2C order validation failed: Too many items. Max: {Max}, Requested: {Requested}",
                B2C_MAX_ITEMS, request.Items.Count);

            return OrderValidationResult.Failed(
                $"B2C orders cannot have more than {B2C_MAX_ITEMS} items. You have {request.Items.Count} items.",
                "ORDER_4001");
        }

        // Validate customer exists
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
        if (customer == null)
        {
            return OrderValidationResult.Failed(
                $"Customer {request.CustomerId} not found",
                "ORDER_4002");
        }

        _logger.LogInformation("B2C order validation passed for customer {CustomerId}", request.CustomerId);
        return OrderValidationResult.Success();
    }

    public async Task<OrderValidationResult> ValidateB2BOrderAsync(CreateOrderRequest request, int companyId)
    {
        _logger.LogInformation("Validating B2B order for company {CompanyId}", companyId);

        // Validate company exists
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
        {
            return OrderValidationResult.Failed(
                $"Company {companyId} not found",
                "ORDER_4003");
        }

        // BR-010: Minimum order amount
        var totalAmount = request.Items.Sum(i => i.UnitPrice * i.Quantity);
        if (totalAmount < B2B_MIN_ORDER_AMOUNT)
        {
            _logger.LogWarning(
                "B2B order validation failed: Order amount too low. Min: {Min}, Actual: {Actual}",
                B2B_MIN_ORDER_AMOUNT, totalAmount);

            return OrderValidationResult.Failed(
                $"B2B orders must be at least {B2B_MIN_ORDER_AMOUNT:C}. Your order total is {totalAmount:C}.",
                "ORDER_4004");
        }

        // TODO: BR-010: Credit limit check
        // Add CreditLimit field to Company entity and implement:
        // if (company.CreditLimit.HasValue)
        // {
        //     var outstandingBalance = await GetCompanyOutstandingBalanceAsync(companyId);
        //     var availableCredit = company.CreditLimit.Value - outstandingBalance;
        //     if (totalAmount > availableCredit)
        //     {
        //         return OrderValidationResult.Failed(
        //             $"Insufficient credit. Available: {availableCredit:C}, Required: {totalAmount:C}",
        //             "ORDER_4005");
        //     }
        // }

        _logger.LogInformation("B2B order validation passed for company {CompanyId}", companyId);
        return OrderValidationResult.Success();
    }

    // TODO: Implement when CreditLimit is added to Company entity
    /*
    private async Task<decimal> GetCompanyOutstandingBalanceAsync(int companyId)
    {
        var orders = await _unitOfWork.Orders.GetByCompanyIdAsync(companyId);
        
        var outstandingBalance = orders
            .Where(o => o.OrderStatus == Domain.Enums.OrderStatus.Pending ||
                       o.OrderStatus == Domain.Enums.OrderStatus.Approved ||
                       o.OrderStatus == Domain.Enums.OrderStatus.Processing)
            .Sum(o => o.TotalAmount);

        return outstandingBalance;
    }
    */
}
