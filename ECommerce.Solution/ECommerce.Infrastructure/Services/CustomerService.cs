using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
    }

    public async Task<CustomerDto?> GetByApplicationUserIdAsync(string userId)
    {
        var customer = await _unitOfWork.Customers
            .Query()
            .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
        
        return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
    {
        var customers = await _unitOfWork.Customers
            .Query()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    public async Task<CustomerDto> UpdateAsync(int id, UpdateCustomerRequest request)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer == null)
            throw new InvalidOperationException($"Customer {id} not found");

        _mapper.Map(request, customer);
        customer.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CustomerDto>(customer);
    }
}
