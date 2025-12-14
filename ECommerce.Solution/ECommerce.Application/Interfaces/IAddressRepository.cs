using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IAddressRepository : IRepository<Address>
{
    Task<IEnumerable<Address>> GetByCustomerIdAsync(int customerId);
    Task<Address?> GetDefaultAddressAsync(int customerId);
}
