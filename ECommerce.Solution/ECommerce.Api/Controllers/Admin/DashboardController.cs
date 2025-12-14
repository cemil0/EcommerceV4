using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/Admin/[controller]")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IAdminOrderService _adminOrderService;

    public DashboardController(IAdminOrderService adminOrderService)
    {
        _adminOrderService = adminOrderService;
    }

    [HttpGet]
    public async Task<ActionResult<AdminDashboardDto>> GetDashboard([FromQuery] DateTime? startDate = null)
    {
        var dashboardData = await _adminOrderService.GetDashboardDataAsync(startDate);
        return Ok(dashboardData);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummary>> GetSummary()
    {
        var dashboard = await GetDashboard();
        var result = dashboard.Result as OkObjectResult;
        var dto = result?.Value as AdminDashboardDto;
        return Ok(dto?.Summary);
    }
}
