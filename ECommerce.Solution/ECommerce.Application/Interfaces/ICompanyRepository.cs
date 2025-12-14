using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface ICompanyRepository : IRepository<Company>
{
    Task<Company?> GetByTaxNumberAsync(string taxNumber);
    Task UpdateBalanceAsync(int companyId, decimal amount, string transactionType, string description, bool isForce = false);
}
