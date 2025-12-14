using AutoMapper;
using ECommerce.Application.Configuration;
using ECommerce.Application.Constants;
using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using ECommerce.Infrastructure.Extensions;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;
    private readonly ILogger<ProductService> _logger;
    private readonly CacheOptions _cacheOptions;

    public ProductService(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        ICacheService cache,
        ILogger<ProductService> logger,
        IOptions<CacheOptions> cacheOptions)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
        _cacheOptions = cacheOptions.Value;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        if (!_cacheOptions.EnableCaching)
            return await GetProductFromDbAsync(id);

        var cacheKey = CacheKeys.Product(id);
        var expiration = TimeSpan.FromMinutes(_cacheOptions.ProductCacheMinutes);

        try
        {
            return await _cache.GetOrSetAsync(
                cacheKey,
                () => GetProductFromDbAsync(id),
                expiration
            );
        }
        catch (Exception ex)
        {
            // Cache failed, fallback to DB
            _logger.LogWarning(ex, "Cache failed for product {ProductId}, falling back to database", id);
            return await GetProductFromDbAsync(id);
        }
    }

    public async Task<ProductDto?> GetBySlugAsync(string slug)
    {
        // Slug-based queries are not cached (less predictable keys)
        var product = await _unitOfWork.Products.GetBySlugAsync(slug);
        return product != null ? _mapper.Map<ProductDto>(product) : null;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        if (!_cacheOptions.EnableCaching)
            return await GetAllProductsFromDbAsync();

        var cacheKey = CacheKeys.ProductList(1, 100); // Default page
        var expiration = TimeSpan.FromMinutes(_cacheOptions.ProductCacheMinutes);

        return await _cache.GetOrSetAsync(
            cacheKey,
            GetAllProductsFromDbAsync,
            expiration
        );
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(int categoryId)
    {
        if (!_cacheOptions.EnableCaching)
            return await GetProductsByCategoryFromDbAsync(categoryId);

        var cacheKey = CacheKeys.ProductsByCategory(categoryId);
        var expiration = TimeSpan.FromMinutes(_cacheOptions.ProductCacheMinutes);

        return await _cache.GetOrSetAsync(
            cacheKey,
            () => GetProductsByCategoryFromDbAsync(categoryId),
            expiration
        );
    }

    public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync(int count = 10)
    {
        if (!_cacheOptions.EnableCaching)
            return await GetFeaturedProductsFromDbAsync(count);

        var cacheKey = $"{CacheKeys.ProductPrefix}Featured:{count}";
        var expiration = TimeSpan.FromMinutes(_cacheOptions.ProductCacheMinutes);

        return await _cache.GetOrSetAsync(
            cacheKey,
            () => GetFeaturedProductsFromDbAsync(count),
            expiration
        );
    }

    public async Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm)
    {
        // Search results are not cached (too many variations)
        var products = await _unitOfWork.Products
            .Query()
            .Where(p => p.IsActive &&
                       (p.ProductName.Contains(searchTerm) ||
                        p.Brand!.Contains(searchTerm) ||
                        p.SKU.Contains(searchTerm)))
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    // Private helper methods for database access
    private async Task<ProductDto?> GetProductFromDbAsync(int id)
    {
        var product = await _unitOfWork.Products
            .Query()
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .FirstOrDefaultAsync(p => p.ProductId == id);
            
        return product != null ? _mapper.Map<ProductDto>(product) : null;
    }

    private async Task<IEnumerable<ProductDto>> GetAllProductsFromDbAsync()
    {
        var products = await _unitOfWork.Products
            .Query()
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    private async Task<IEnumerable<ProductDto>> GetProductsByCategoryFromDbAsync(int categoryId)
    {
        var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    private async Task<IEnumerable<ProductDto>> GetFeaturedProductsFromDbAsync(int count)
    {
        var products = await _unitOfWork.Products.GetFeaturedProductsAsync(count);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    // NEW: Paginated methods
    public async Task<PagedResponse<ProductDto>> GetPagedAsync(PagedRequest request)
    {
        var query = _unitOfWork.Products
            .Query()
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .ApplySorting(request.SortBy ?? "ProductName", request.SortDescending);

        var pagedProducts = await query.ToPagedResponseAsync(request);
        
        var productDtos = _mapper.Map<List<ProductDto>>(pagedProducts.Data);
        
        return new PagedResponse<ProductDto>(
            productDtos,
            pagedProducts.Page,
            pagedProducts.PageSize,
            pagedProducts.TotalCount);
    }

    public async Task<PagedResponse<ProductDto>> GetPagedByCategoryAsync(int categoryId, PagedRequest request)
    {
        var query = _unitOfWork.Products
            .Query()
            .Where(p => p.IsActive && p.CategoryId == categoryId)
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .ApplySorting(request.SortBy ?? "ProductName", request.SortDescending);

        var pagedProducts = await query.ToPagedResponseAsync(request);
        
        var productDtos = _mapper.Map<List<ProductDto>>(pagedProducts.Data);
        
        return new PagedResponse<ProductDto>(
            productDtos,
            pagedProducts.Page,
            pagedProducts.PageSize,
            pagedProducts.TotalCount);
    }

    public async Task<PagedResponse<ProductDto>> SearchPagedAsync(string searchTerm, PagedRequest request)
    {
        var query = _unitOfWork.Products
            .Query()
            .Where(p => p.IsActive &&
                       (p.ProductName.Contains(searchTerm) ||
                        p.Brand!.Contains(searchTerm) ||
                        p.SKU.Contains(searchTerm)))
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .ApplySorting(request.SortBy ?? "ProductName", request.SortDescending);

        var pagedProducts = await query.ToPagedResponseAsync(request);
        
        var productDtos = _mapper.Map<List<ProductDto>>(pagedProducts.Data);
        
        return new PagedResponse<ProductDto>(
            productDtos,
            pagedProducts.Page,
            pagedProducts.PageSize,
            pagedProducts.TotalCount);
    }
}
