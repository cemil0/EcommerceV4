using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/Admin/[controller]")]
[Authorize(Roles = "Admin")]
public class OrdersController : ControllerBase
{
    private readonly IAdminOrderService _adminOrderService;

    public OrdersController(IAdminOrderService adminOrderService)
    {
        _adminOrderService = adminOrderService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AdminOrderDto>>> GetOrders([FromQuery] OrderFilterRequest request)
    {
        var result = await _adminOrderService.GetOrdersAsync(request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AdminOrderDetailDto>> GetOrder(int id)
    {
        var result = await _adminOrderService.GetOrderDetailAsync(id);
        if (result == null)
            return NotFound($"Order {id} not found");

        return Ok(result);
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<OrderStatisticsDto>> GetStatistics()
    {
        var result = await _adminOrderService.GetStatisticsAsync();
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateRequest request)
    {
        var success = await _adminOrderService.UpdateOrderStatusAsync(id, request.NewStatus);
        if (!success)
            return NotFound($"Order {id} not found or invalid status");

        return Ok($"Order status updated to {request.NewStatus}");
    }

    // Order Timeline
    [HttpGet("{id}/timeline")]
    public async Task<ActionResult<OrderTimelineDto>> GetOrderTimeline(int id)
    {
        var result = await _adminOrderService.GetOrderTimelineAsync(id);
        if (result == null)
            return NotFound($"Order {id} not found");

        return Ok(result);
    }

    // Order Refund
    [HttpPost("{orderId}/refund")]
    public async Task<IActionResult> ProcessRefund(int orderId, [FromBody] RefundRequest request)
    {
        var result = await _adminOrderService.ProcessRefundAsync(orderId, request);
        if (!result) return BadRequest("Refund failed");
        return Ok();
    }

    [HttpGet("chart")]
    public async Task<IActionResult> GetSalesChart([FromQuery] int days = 30)
    {
        var data = await _adminOrderService.GetSalesChartDataAsync(days);
        return Ok(data);
    }

    [HttpGet("top-selling")]
    public async Task<IActionResult> GetTopSellingProducts([FromQuery] int count = 5, [FromQuery] DateTime? startDate = null)
    {
        var data = await _adminOrderService.GetTopSellingProductsAsync(count, startDate);
        return Ok(data);
    }

    // Order Approval (B2B)
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApproveOrder(int id)
    {
        var success = await _adminOrderService.ApproveOrderAsync(id);
        if (!success)
            return NotFound($"Order {id} not found");

        return Ok("Order approved successfully");
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult> RejectOrder(int id, [FromQuery] string reason)
    {
        var success = await _adminOrderService.RejectOrderAsync(id, reason);
        if (!success)
            return NotFound($"Order {id} not found");

        return Ok($"Order rejected: {reason}");
    }
    [HttpPost("seed")]
    [AllowAnonymous]
    public async Task<IActionResult> SeedOrders()
    {
        try
        {
            await _adminOrderService.SeedHistoricalOrdersAsync();
            return Ok("Historical orders seeded successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
