using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.DTOs.Common;

namespace ECommerce.Application.Interfaces.Services;

public interface IAdminProductService
{
    Task<PagedResponse<AdminProductDto>> GetProductsAsync(PagedRequest request);
    Task<AdminProductDto> CreateProductAsync(CreateProductRequest request);
    Task<AdminProductDto?> UpdateProductAsync(int productId, UpdateProductRequest request);
    Task<AdminProductDetailDto?> GetProductDetailAsync(int productId);
    Task<bool> ActivateProductAsync(int productId);
    Task<bool> DeactivateProductAsync(int productId);
    Task<bool> DeleteProductAsync(int productId);
    
    // Variant CRUD
    Task<AdminVariantDto?> CreateVariantAsync(int productId, CreateVariantRequest request);
    Task<bool> UpdateVariantAsync(int productId, int variantId, UpdateVariantRequest request);
    Task<bool> DeleteVariantAsync(int productId, int variantId);
    
    // Stock Management
    Task<bool> UpdateStockAsync(int productId, int variantId, UpdateStockRequest request);
    Task<bool> BulkUpdateStockAsync(BulkStockUpdateRequest request);
    
    // Image Management
    Task<ProductImageDto> UploadImageAsync(int productId, Stream imageStream, string fileName, UploadImageRequest request);
    Task<IEnumerable<ProductImageDto>> GetProductImagesAsync(int productId);
    Task<bool> DeleteImageAsync(int productId, int imageId);
    Task<bool> SetPrimaryImageAsync(int productId, int imageId);
    Task<bool> ReorderImagesAsync(int productId, ReorderImagesRequest request);
    Task<IEnumerable<ProductImageDto>> BulkUploadImagesAsync(int productId, IEnumerable<(Stream stream, string fileName)> files);
}

public interface IAdminOrderService
{
    Task<PagedResponse<AdminOrderDto>> GetOrdersAsync(OrderFilterRequest request);
    Task<AdminOrderDetailDto?> GetOrderDetailAsync(int orderId);
    Task<OrderStatisticsDto> GetStatisticsAsync();
    Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus);
    
    // Order Timeline
    Task<OrderTimelineDto?> GetOrderTimelineAsync(int orderId);
    
    // Order Refund
    Task<bool> ProcessRefundAsync(int orderId, RefundRequest request);
    Task<SalesChartDto> GetSalesChartDataAsync(int days = 30);
    Task<List<TopSellingProductDto>> GetTopSellingProductsAsync(int count = 5, DateTime? startDate = null);
    
    // Order Approval (B2B)
    Task<bool> ApproveOrderAsync(int orderId);
    Task<bool> RejectOrderAsync(int orderId, string reason);
    
    // Dashboard
    Task<AdminDashboardDto> GetDashboardDataAsync(DateTime? startDate = null);
    
    // Seeding
    Task<bool> SeedHistoricalOrdersAsync();
}
