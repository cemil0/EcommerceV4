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
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Get current customer profile
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get my profile", Description = "Retrieve current customer's profile")]
    public async Task<ActionResult<CustomerDto>> GetMyProfile()
    {
        var customerId = User.GetCustomerId();
        if (!customerId.HasValue)
            return Unauthorized("Customer ID not found in token");

        var customer = await _customerService.GetByIdAsync(customerId.Value);
        
        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    /// <summary>
    /// Update current customer profile
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Update my profile", Description = "Update current customer's profile")]
    public async Task<ActionResult<CustomerDto>> UpdateMyProfile([FromBody] UpdateCustomerRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customerId = User.GetCustomerId();
        if (!customerId.HasValue)
            return Unauthorized("Customer ID not found in token");

        try
        {
            var customer = await _customerService.UpdateAsync(customerId.Value, request);
            return Ok(customer);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all customers (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Get all customers", Description = "Retrieve all customers (Admin only)")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers()
    {
        var customers = await _customerService.GetAllAsync();
        return Ok(customers);
    }

    /// <summary>
    /// Get customer by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get customer by ID", Description = "Retrieve a specific customer (Admin only)")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        
        if (customer == null)
            return NotFound();

        return Ok(customer);
    }
}
