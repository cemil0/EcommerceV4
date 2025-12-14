using ECommerce.Application.Interfaces.Repositories;

namespace ECommerce.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Repositories
    IProductRepository Products { get; }
    IProductVariantRepository ProductVariants { get; }
    IProductImageRepository ProductImages { get; }
    ICategoryRepository Categories { get; }
    ICustomerRepository Customers { get; }
    ICompanyRepository Companies { get; }
    IAddressRepository Addresses { get; }
    ICartRepository Carts { get; }
    ICartItemRepository CartItems { get; }
    IOrderRepository Orders { get; }
    IOrderItemRepository OrderItems { get; }
    IOrderStatusHistoryRepository OrderStatusHistory { get; }

    // Transaction
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
