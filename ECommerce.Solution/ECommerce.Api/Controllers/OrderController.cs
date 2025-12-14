using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var order = await _orderService.GetByIdAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    /// <summary>
    /// Get order by order number
    /// </summary>
    [HttpGet("by-number/{orderNumber}")]
    public async Task<ActionResult<OrderDto>> GetOrderByNumber(string orderNumber)
    {
        var order = await _orderService.GetByOrderNumberAsync(orderNumber);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    /// <summary>
    /// Get all orders for a customer
    /// </summary>
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetCustomerOrders(int customerId)
    {
        var orders = await _orderService.GetByCustomerIdAsync(customerId);
        return Ok(orders);
    }

    /// <summary>
    /// Create B2C order (BR-009)
    /// </summary>
    [HttpPost("b2c")]
    public async Task<ActionResult<OrderDto>> CreateB2COrder([FromBody] CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            request.OrderType = "B2C";
            var order = await _orderService.CreateB2COrderAsync(request);
            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create B2B order (BR-010)
    /// </summary>
    [HttpPost("b2b")]
    public async Task<ActionResult<OrderDto>> CreateB2BOrder([FromBody] CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!request.CompanyId.HasValue)
        {
            return BadRequest(new { error = "CompanyId is required for B2B orders" });
        }

        try
        {
            request.OrderType = "B2B";
            var order = await _orderService.CreateB2BOrderAsync(request);
            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generate order number (BR-011)
    /// </summary>
    [HttpGet("generate-number")]
    public async Task<ActionResult<string>> GenerateOrderNumber()
    {
        var orderNumber = await _orderService.GenerateOrderNumberAsync();
        return Ok(new { orderNumber });
    }
}
