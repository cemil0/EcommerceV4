using AutoMapper;
using ECommerce.Application.Configuration;
using ECommerce.Application.Constants;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;
    private readonly CacheOptions _cacheOptions;

    public CategoryService(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        ICacheService cache,
        IOptions<CacheOptions> cacheOptions)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
        _cacheOptions = cacheOptions.Value;
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        if (!_cacheOptions.EnableCaching)
            return await GetCategoryFromDbAsync(id);

        try
        {
            var cacheKey = CacheKeys.Category(id);
            var expiration = TimeSpan.FromMinutes(_cacheOptions.CategoryCacheMinutes);

            return await _cache.GetOrSetAsync(
                cacheKey,
                () => GetCategoryFromDbAsync(id),
                expiration
            );
        }
        catch
        {
            return await GetCategoryFromDbAsync(id);
        }
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug)
    {
        // Slug-based queries are not cached
        var category = await _unitOfWork.Categories.GetBySlugAsync(slug);
        return category != null ? _mapper.Map<CategoryDto>(category) : null;
    }

    public async Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync()
    {
        if (!_cacheOptions.EnableCaching)
            return await GetRootCategoriesFromDbAsync();

        try
        {
            var cacheKey = CacheKeys.CategoryList();
            var expiration = TimeSpan.FromMinutes(_cacheOptions.CategoryCacheMinutes);

            return await _cache.GetOrSetAsync(
                cacheKey,
                GetRootCategoriesFromDbAsync,
                expiration
            );
        }
        catch
        {
            return await GetRootCategoriesFromDbAsync();
        }
    }

    public async Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(int parentCategoryId)
    {
        if (!_cacheOptions.EnableCaching)
            return await GetSubCategoriesFromDbAsync(parentCategoryId);

        try
        {
            var cacheKey = $"{CacheKeys.CategoryPrefix}SubCategories:{parentCategoryId}";
            var expiration = TimeSpan.FromMinutes(_cacheOptions.CategoryCacheMinutes);

            return await _cache.GetOrSetAsync(
                cacheKey,
                () => GetSubCategoriesFromDbAsync(parentCategoryId),
                expiration
            );
        }
        catch
        {
            return await GetSubCategoriesFromDbAsync(parentCategoryId);
        }
    }

    // Private helper methods
    private async Task<CategoryDto?> GetCategoryFromDbAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        return category != null ? _mapper.Map<CategoryDto>(category) : null;
    }

    private async Task<IEnumerable<CategoryDto>> GetRootCategoriesFromDbAsync()
    {
        var categories = await _unitOfWork.Categories.GetRootCategoriesAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    private async Task<IEnumerable<CategoryDto>> GetSubCategoriesFromDbAsync(int parentCategoryId)
    {
        var categories = await _unitOfWork.Categories.GetSubCategoriesAsync(parentCategoryId);
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }
}
