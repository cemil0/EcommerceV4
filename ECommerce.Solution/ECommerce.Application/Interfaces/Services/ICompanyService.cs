using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces.Services;

public interface ICompanyService
{
    Task<CompanyDto?> GetByIdAsync(int id);
    Task<CompanyDto?> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<CompanyDto>> GetAllAsync();
    Task<CompanyDto> UpdateAsync(int id, UpdateCompanyRequest request);
}
