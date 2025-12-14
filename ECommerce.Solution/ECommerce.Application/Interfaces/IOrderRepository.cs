using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<Order>> GetByCompanyIdAsync(int companyId);
    Task<int> GetCountForYearAsync(int year);
    Task<IEnumerable<Order>> GetAllWithDetailsAsync();
    Task<Order?> GetByIdWithDetailsAsync(int orderId);
}
