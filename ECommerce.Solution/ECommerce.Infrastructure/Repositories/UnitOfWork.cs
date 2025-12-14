using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerce.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ECommerceDbContext _context;
    private IDbContextTransaction? _transaction;

    // Repositories
    public IProductRepository Products { get; }
    public IProductVariantRepository ProductVariants { get; }
    public IProductImageRepository ProductImages { get; }
    public ICategoryRepository Categories { get; }
    public ICustomerRepository Customers { get; }
    public ICompanyRepository Companies { get; }
    public IAddressRepository Addresses { get; }
    public ICartRepository Carts { get; }
    public ICartItemRepository CartItems { get; }
    public IOrderRepository Orders { get; }
    public IOrderItemRepository OrderItems { get; }
    public IOrderStatusHistoryRepository OrderStatusHistory { get; }

    public UnitOfWork(
        ECommerceDbContext context,
        IProductRepository products,
        IProductVariantRepository productVariants,
        IProductImageRepository productImages,
        ICategoryRepository categories,
        ICustomerRepository customers,
        ICompanyRepository companies,
        IAddressRepository addresses,
        ICartRepository carts,
        ICartItemRepository cartItems,
        IOrderRepository orders,
        IOrderItemRepository orderItems,
        IOrderStatusHistoryRepository orderStatusHistory)
    {
        _context = context;
        Products = products;
        ProductVariants = productVariants;
        ProductImages = productImages;
        Categories = categories;
        Customers = customers;
        Companies = companies;
        Addresses = addresses;
        Carts = carts;
        CartItems = cartItems;
        Orders = orders;
        OrderItems = orderItems;
        OrderStatusHistory = orderStatusHistory;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
