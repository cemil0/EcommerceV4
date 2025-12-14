using ECommerce.Api.Extensions;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressesController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    /// <summary>
    /// Get my addresses
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AddressDto>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Get my addresses", Description = "Retrieve all addresses for the current customer")]
    public async Task<ActionResult<IEnumerable<AddressDto>>> GetMyAddresses()
    {
        var customerId = User.GetCustomerId();
        if (!customerId.HasValue)
            return Unauthorized("Customer ID not found in token");

        var addresses = await _addressService.GetByCustomerIdAsync(customerId.Value);
        return Ok(addresses);
    }

    /// <summary>
    /// Get address by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get address by ID", Description = "Retrieve a specific address")]
    public async Task<ActionResult<AddressDto>> GetAddress(int id)
    {
        var address = await _addressService.GetByIdAsync(id);
        
        if (address == null)
            return NotFound();

        // Verify ownership
        var customerId = User.GetCustomerId();
        if (customerId.HasValue && address.CustomerId != customerId.Value)
            return Forbid();

        return Ok(address);
    }

    /// <summary>
    /// Create new address
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Create address", Description = "Create a new address for the current customer")]
    public async Task<ActionResult<AddressDto>> CreateAddress([FromBody] CreateAddressRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customerId = User.GetCustomerId();
        if (!customerId.HasValue)
            return Unauthorized("Customer ID not found in token");

        request.CustomerId = customerId.Value;

        try
        {
            var address = await _addressService.CreateAsync(request);
            return CreatedAtAction(nameof(GetAddress), new { id = address.AddressId }, address);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update address
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Update address", Description = "Update an existing address")]
    public async Task<ActionResult<AddressDto>> UpdateAddress(int id, [FromBody] UpdateAddressRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Verify ownership
        var existingAddress = await _addressService.GetByIdAsync(id);
        if (existingAddress == null)
            return NotFound();

        var customerId = User.GetCustomerId();
        if (customerId.HasValue && existingAddress.CustomerId != customerId.Value)
            return Forbid();

        try
        {
            var address = await _addressService.UpdateAsync(id, request);
            return Ok(address);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete address
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete address", Description = "Delete an address")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        // Verify ownership
        var existingAddress = await _addressService.GetByIdAsync(id);
        if (existingAddress == null)
            return NotFound();

        var customerId = User.GetCustomerId();
        if (customerId.HasValue && existingAddress.CustomerId != customerId.Value)
            return Forbid();

        try
        {
            await _addressService.DeleteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Set default address
    /// </summary>
    [HttpPut("{id}/set-default")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Set default address", Description = "Set an address as the default")]
    public async Task<ActionResult<AddressDto>> SetDefaultAddress(int id)
    {
        var customerId = User.GetCustomerId();
        if (!customerId.HasValue)
            return Unauthorized("Customer ID not found in token");

        try
        {
            var address = await _addressService.SetDefaultAsync(id, customerId.Value);
            return Ok(address);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
