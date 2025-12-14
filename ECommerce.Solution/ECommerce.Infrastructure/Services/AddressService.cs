using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class AddressService : IAddressService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddressService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AddressDto>> GetByCustomerIdAsync(int customerId)
    {
        var addresses = await _unitOfWork.Addresses
            .Query()
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.AddressTitle)
            .ToListAsync();

        return _mapper.Map<IEnumerable<AddressDto>>(addresses);
    }

    public async Task<AddressDto?> GetByIdAsync(int id)
    {
        var address = await _unitOfWork.Addresses.GetByIdAsync(id);
        return address != null ? _mapper.Map<AddressDto>(address) : null;
    }

    public async Task<AddressDto> CreateAsync(CreateAddressRequest request)
    {
        var address = _mapper.Map<Address>(request);
        address.CreatedAt = DateTime.UtcNow;
        address.UpdatedAt = DateTime.UtcNow;

        // If this is the first address or marked as default, set it as default
        var existingAddresses = await _unitOfWork.Addresses
            .Query()
            .Where(a => a.CustomerId == request.CustomerId)
            .ToListAsync();

        if (!existingAddresses.Any())
        {
            address.IsDefault = true;
        }

        await _unitOfWork.Addresses.AddAsync(address);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AddressDto>(address);
    }

    public async Task<AddressDto> UpdateAsync(int id, UpdateAddressRequest request)
    {
        var address = await _unitOfWork.Addresses.GetByIdAsync(id);
        if (address == null)
            throw new InvalidOperationException($"Address {id} not found");

        _mapper.Map(request, address);
        address.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Addresses.Update(address);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AddressDto>(address);
    }

    public async Task DeleteAsync(int id)
    {
        var address = await _unitOfWork.Addresses.GetByIdAsync(id);
        if (address == null)
            throw new InvalidOperationException($"Address {id} not found");

        // If deleting default address, set another as default
        if (address.IsDefault)
        {
            var otherAddress = await _unitOfWork.Addresses
                .Query()
                .Where(a => a.CustomerId == address.CustomerId && a.AddressId != id)
                .FirstOrDefaultAsync();

            if (otherAddress != null)
            {
                otherAddress.IsDefault = true;
                _unitOfWork.Addresses.Update(otherAddress);
            }
        }

        _unitOfWork.Addresses.Remove(address);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<AddressDto> SetDefaultAsync(int id, int customerId)
    {
        var address = await _unitOfWork.Addresses.GetByIdAsync(id);
        if (address == null || address.CustomerId != customerId)
            throw new InvalidOperationException($"Address {id} not found or doesn't belong to customer {customerId}");

        // Unset all other default addresses for this customer
        var otherAddresses = await _unitOfWork.Addresses
            .Query()
            .Where(a => a.CustomerId == customerId && a.AddressId != id && a.IsDefault)
            .ToListAsync();

        foreach (var other in otherAddresses)
        {
            other.IsDefault = false;
            _unitOfWork.Addresses.Update(other);
        }

        address.IsDefault = true;
        address.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Addresses.Update(address);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AddressDto>(address);
    }
}
