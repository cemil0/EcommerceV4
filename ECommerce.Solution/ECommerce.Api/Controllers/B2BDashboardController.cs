using ECommerce.Api.Extensions;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class B2BDashboardController : ControllerBase
{
    private readonly IB2BDashboardService _dashboardService;
    private readonly ICustomerService _customerService;

    public B2BDashboardController(IB2BDashboardService dashboardService, ICustomerService customerService)
    {
        _dashboardService = dashboardService;
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<B2BDashboardDto>> GetDashboard()
    {
        var customerId = User.GetCustomerId();
        if (!customerId.HasValue)
            return Unauthorized("Customer ID not found");

        var customer = await _customerService.GetByIdAsync(customerId.Value);
        if (customer?.CompanyId == null)
            return BadRequest("No company associated with this customer");

        var dashboard = await _dashboardService.GetDashboardAsync(customer.CompanyId.Value);
        return Ok(dashboard);
    }

    [HttpGet("financial")]
    public async Task<ActionResult<CompanyFinancialSummary>> GetFinancial()
    {
        var customerId = User.GetCustomerId();
        if (!customerId.HasValue)
            return Unauthorized("Customer ID not found");

        var customer = await _customerService.GetByIdAsync(customerId.Value);
        if (customer?.CompanyId == null)
            return BadRequest("No company associated with this customer");

        var financial = await _dashboardService.GetFinancialSummaryAsync(customer.CompanyId.Value);
        return Ok(financial);
    }
}
