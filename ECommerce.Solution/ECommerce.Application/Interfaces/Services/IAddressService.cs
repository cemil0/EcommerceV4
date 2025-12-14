using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces.Services;

public interface IAddressService
{
    Task<IEnumerable<AddressDto>> GetByCustomerIdAsync(int customerId);
    Task<AddressDto?> GetByIdAsync(int id);
    Task<AddressDto> CreateAsync(CreateAddressRequest request);
    Task<AddressDto> UpdateAsync(int id, UpdateAddressRequest request);
    Task DeleteAsync(int id);
    Task<AddressDto> SetDefaultAsync(int id, int customerId);
}
