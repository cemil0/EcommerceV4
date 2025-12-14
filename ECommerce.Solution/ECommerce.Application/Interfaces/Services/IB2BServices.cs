using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces.Services;

public interface IB2BCreditService
{
    Task<bool> CheckCreditLimitAsync(int companyId, decimal orderAmount);
    Task<decimal> GetAvailableCreditAsync(int companyId);
    Task ReserveCreditAsync(int companyId, decimal amount);
    Task ReleaseCreditAsync(int companyId, decimal amount);
}

public interface IB2BDashboardService
{
    Task<B2BDashboardDto> GetDashboardAsync(int companyId);
    Task<CompanyFinancialSummary> GetFinancialSummaryAsync(int companyId);
}
