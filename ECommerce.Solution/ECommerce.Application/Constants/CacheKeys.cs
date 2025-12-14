namespace ECommerce.Application.Constants;

/// <summary>
/// Cache key constants with namespace prefix for Redis
/// </summary>
public static class CacheKeys
{
    private const string Namespace = "ECommerce";
    
    // Prefixes with namespace
    public const string ProductPrefix = $"{Namespace}:Product:";
    public const string CategoryPrefix = $"{Namespace}:Category:";
    public const string UserPrefix = $"{Namespace}:User:";
    
    // Product keys
    public static string Product(int id) => $"{ProductPrefix}{id}";
    
    public static string ProductList(int page, int pageSize, string? search = null) 
        => $"{ProductPrefix}List:{page}:{pageSize}:{search ?? "all"}";
    
    public static string ProductsByCategory(int categoryId) 
        => $"{ProductPrefix}Category:{categoryId}";
    
    // Category keys
    public static string Category(int id) => $"{CategoryPrefix}{id}";
    
    public static string CategoryList() => $"{CategoryPrefix}List";
    
    public static string CategoryTree() => $"{CategoryPrefix}Tree";
    
    // User keys
    public static string UserCart(string userId) => $"{UserPrefix}Cart:{userId}";
    
    public static string UserWishlist(string userId) => $"{UserPrefix}Wishlist:{userId}";
}
