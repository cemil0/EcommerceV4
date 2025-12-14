using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto?> GetByApplicationUserIdAsync(string userId);
    Task<IEnumerable<CustomerDto>> GetAllAsync();
    Task<CustomerDto> UpdateAsync(int id, UpdateCustomerRequest request);
}
