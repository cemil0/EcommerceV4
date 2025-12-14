using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .Include(o => o.Customer)
            .Include(o => o.BillingAddress)
            .Include(o => o.ShippingAddress)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByCompanyIdAsync(int companyId)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .Where(o => o.CompanyId == companyId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<int> GetCountForYearAsync(int year)
    {
        return await _dbSet
            .Where(o => o.OrderDate.Year == year)
            .CountAsync();
    }

    public async Task<IEnumerable<Order>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(o => o.Customer)
            .ThenInclude(c => c.Company)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int orderId)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .ThenInclude(c => c.Company)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.BillingAddress)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }
}
