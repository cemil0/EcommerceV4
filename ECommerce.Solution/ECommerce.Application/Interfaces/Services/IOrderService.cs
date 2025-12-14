using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;

namespace ECommerce.Application.Interfaces.Services;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(int id);
    Task<OrderDto?> GetByOrderNumberAsync(string orderNumber);
    Task<IEnumerable<OrderDto>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<OrderDto>> GetAllAsync();
    Task<OrderDto> CreateB2COrderAsync(CreateOrderRequest request);
    Task<OrderDto> CreateB2BOrderAsync(CreateOrderRequest request);
    Task<string> GenerateOrderNumberAsync();
    
    // NEW: Paginated methods
    Task<PagedResponse<OrderDto>> GetPagedAsync(PagedRequest request);
    Task<PagedResponse<OrderDto>> GetPagedByCustomerAsync(int customerId, PagedRequest request);
}
