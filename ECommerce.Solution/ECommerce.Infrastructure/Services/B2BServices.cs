using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class B2BCreditService : IB2BCreditService
{
    private readonly IUnitOfWork _unitOfWork;

    public B2BCreditService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// ERP-LEVEL: Check credit limit (simplified version)
    /// NOTE: For full ACID guarantee with UPDLOCK, use stored procedure
    /// </summary>
    public async Task<bool> CheckCreditLimitAsync(int companyId, decimal orderAmount)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null) return false;

        var availableCredit = company.CreditLimit - company.CurrentBalance;
        return orderAmount <= availableCredit;
    }

    public async Task<decimal> GetAvailableCreditAsync(int companyId)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null) return 0;

        return company.CreditLimit - company.CurrentBalance;
    }

    public async Task ReserveCreditAsync(int companyId, decimal amount)
    {
        // Use Stored Procedure for atomic update and lock
        await _unitOfWork.Companies.UpdateBalanceAsync(companyId, amount, "DEBIT", "Order Reservation");
    }

    public async Task ReleaseCreditAsync(int companyId, decimal amount)
    {
        // Use Stored Procedure for atomic update and lock
        await _unitOfWork.Companies.UpdateBalanceAsync(companyId, amount, "CREDIT", "Order Cancel/Release");
    }
}

public class B2BDashboardService : IB2BDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public B2BDashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<B2BDashboardDto> GetDashboardAsync(int companyId)
    {
        var financial = await GetFinancialSummaryAsync(companyId);
        var orders = await GetOrderStatisticsAsync(companyId);

        return new B2BDashboardDto
        {
            Financial = financial,
            Orders = orders
        };
    }

    public async Task<CompanyFinancialSummary> GetFinancialSummaryAsync(int companyId)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new InvalidOperationException($"Company {companyId} not found");

        var totalSpent = await _unitOfWork.Orders
            .Query()
            .Where(o => o.CompanyId == companyId && o.OrderStatus == Domain.Enums.OrderStatus.Delivered)
            .SumAsync(o => o.TotalAmount);

        var availableCredit = company.CreditLimit - company.CurrentBalance;
        var usagePercentage = company.CreditLimit > 0
            ? (company.CurrentBalance / company.CreditLimit) * 100
            : 0;

        return new CompanyFinancialSummary
        {
            CreditLimit = company.CreditLimit,
            CurrentBalance = company.CurrentBalance,
            AvailableCredit = availableCredit,
            CreditUsagePercentage = usagePercentage,
            TotalSpent = totalSpent
        };
    }

    private async Task<OrderStatistics> GetOrderStatisticsAsync(int companyId)
    {
        var orders = await _unitOfWork.Orders
            .Query()
            .Where(o => o.CompanyId == companyId)
            .ToListAsync();

        var totalOrders = orders.Count;
        var pendingApproval = orders.Count(o => o.ApprovalStatus == "Pending" || o.ApprovalStatus == "PendingManagerApproval");
        var approved = orders.Count(o => o.ApprovalStatus == "Approved");
        var avgOrderValue = totalOrders > 0 ? orders.Average(o => o.TotalAmount) : 0;

        return new OrderStatistics
        {
            TotalOrders = totalOrders,
            PendingApproval = pendingApproval,
            Approved = approved,
            AverageOrderValue = avgOrderValue
        };
    }
}
