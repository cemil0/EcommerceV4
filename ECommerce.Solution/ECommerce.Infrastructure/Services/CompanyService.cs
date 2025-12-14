using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CompanyService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CompanyDto?> GetByIdAsync(int id)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        return company != null ? _mapper.Map<CompanyDto>(company) : null;
    }

    public async Task<CompanyDto?> GetByCustomerIdAsync(int customerId)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (customer?.CompanyId == null)
            return null;

        var company = await _unitOfWork.Companies.GetByIdAsync(customer.CompanyId.Value);
        return company != null ? _mapper.Map<CompanyDto>(company) : null;
    }

    public async Task<IEnumerable<CompanyDto>> GetAllAsync()
    {
        var companies = await _unitOfWork.Companies
            .Query()
            .OrderBy(c => c.CompanyName)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<CompanyDto>>(companies);
    }

    public async Task<CompanyDto> UpdateAsync(int id, UpdateCompanyRequest request)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        if (company == null)
            throw new InvalidOperationException($"Company {id} not found");

        _mapper.Map(request, company);
        company.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Companies.Update(company);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CompanyDto>(company);
    }
}
