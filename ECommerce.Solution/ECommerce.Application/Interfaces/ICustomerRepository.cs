using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email);
    Task<Customer?> GetByApplicationUserIdAsync(string applicationUserId);
}
