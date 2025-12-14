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
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    /// <summary>
    /// Get my company (B2B users)
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get my company", Description = "Retrieve company for current B2B customer")]
    public async Task<ActionResult<CompanyDto>> GetMyCompany()
    {
        var customerId = User.GetCustomerId();
        if (!customerId.HasValue)
            return Unauthorized("Customer ID not found in token");

        var company = await _companyService.GetByCustomerIdAsync(customerId.Value);
        
        if (company == null)
            return NotFound("No company associated with this customer");

        return Ok(company);
    }

    /// <summary>
    /// Update my company (B2B users)
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Update my company", Description = "Update company information for current B2B customer")]
    public async Task<ActionResult<CompanyDto>> UpdateMyCompany([FromBody] UpdateCompanyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customerId = User.GetCustomerId();
        if (!customerId.HasValue)
            return Unauthorized("Customer ID not found in token");

        var existingCompany = await _companyService.GetByCustomerIdAsync(customerId.Value);
        if (existingCompany == null)
            return NotFound("No company associated with this customer");

        try
        {
            var company = await _companyService.UpdateAsync(existingCompany.CompanyId, request);
            return Ok(company);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all companies (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<CompanyDto>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Get all companies", Description = "Retrieve all companies (Admin only)")]
    public async Task<ActionResult<IEnumerable<CompanyDto>>> GetAllCompanies()
    {
        var companies = await _companyService.GetAllAsync();
        return Ok(companies);
    }

    /// <summary>
    /// Get company by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get company by ID", Description = "Retrieve a specific company (Admin only)")]
    public async Task<ActionResult<CompanyDto>> GetCompany(int id)
    {
        var company = await _companyService.GetByIdAsync(id);
        
        if (company == null)
            return NotFound();

        return Ok(company);
    }
}
