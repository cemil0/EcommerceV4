using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class ProductVariantRepository : Repository<ProductVariant>, IProductVariantRepository
{
    public ProductVariantRepository(ECommerceDbContext context) : base(context) { }

    public async Task<ProductVariant?> GetBySKUAsync(string sku)
    {
        return await _dbSet.FirstOrDefaultAsync(pv => pv.VariantSKU == sku);
    }

    public async Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId)
    {
        return await _dbSet.Where(pv => pv.ProductId == productId && pv.IsActive).ToListAsync();
    }
}

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ECommerceDbContext context) : base(context) { }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<Customer?> GetByApplicationUserIdAsync(string applicationUserId)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId);
    }
}

public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    public CompanyRepository(ECommerceDbContext context) : base(context) { }

    public async Task<Company?> GetByTaxNumberAsync(string taxNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.TaxNumber == taxNumber);
    }

    public async Task UpdateBalanceAsync(int companyId, decimal amount, string transactionType, string description, bool isForce = false)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC usp_UpdateCompanyBalance @CompanyId, @Amount, @TransactionType, @Description, @IsForce",
            new Microsoft.Data.SqlClient.SqlParameter("@CompanyId", companyId),
            new Microsoft.Data.SqlClient.SqlParameter("@Amount", amount),
            new Microsoft.Data.SqlClient.SqlParameter("@TransactionType", transactionType),
            new Microsoft.Data.SqlClient.SqlParameter("@Description", description),
            new Microsoft.Data.SqlClient.SqlParameter("@IsForce", isForce)
        );
    }
}

public class AddressRepository : Repository<Address>, IAddressRepository
{
    public AddressRepository(ECommerceDbContext context) : base(context) { }

    public async Task<IEnumerable<Address>> GetByCustomerIdAsync(int customerId)
    {
        return await _dbSet.Where(a => a.CustomerId == customerId).ToListAsync();
    }

    public async Task<Address?> GetDefaultAddressAsync(int customerId)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.CustomerId == customerId && a.IsDefault);
    }
}

public class CartItemRepository : Repository<CartItem>, ICartItemRepository
{
    public CartItemRepository(ECommerceDbContext context) : base(context) { }

    public async Task<CartItem?> GetByCartAndVariantAsync(int cartId, int productVariantId)
    {
        return await _dbSet.FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductVariantId == productVariantId);
    }
}

public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(ECommerceDbContext context) : base(context) { }

    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
    {
        return await _dbSet.Where(oi => oi.OrderId == orderId).ToListAsync();
    }
}
