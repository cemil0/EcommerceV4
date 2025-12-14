using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Get cart by customer ID or session ID
    /// </summary>
    /// <param name="customerId">Customer ID (for authenticated users)</param>
    /// <param name="sessionId">Session ID (for guest users)</param>
    /// <returns>Cart with items</returns>
    /// <response code="200">Returns the cart</response>
    /// <response code="400">If neither customerId nor sessionId provided</response>
    /// <response code="404">If cart not found</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get cart", Description = "Retrieve cart by customer ID or session ID (anonymous access allowed)")]
    public async Task<ActionResult<CartDto>> GetCart([FromQuery] int? customerId, [FromQuery] string? sessionId)
    {
        // Auto-detect customerId from token if authenticated
        if (!customerId.HasValue && User.Identity?.IsAuthenticated == true)
        {
            customerId = User.GetCustomerId();
        }

        if (!customerId.HasValue && string.IsNullOrEmpty(sessionId))
        {
            return BadRequest("Either customerId or sessionId must be provided");
        }

        var cart = await _cartService.GetCartAsync(customerId, sessionId);

        if (cart == null)
        {
            return NotFound();
        }

        return Ok(cart);
    }

    /// <summary>
    /// Add item to cart
    /// </summary>
    /// <param name="request">Add to cart request containing product variant ID and quantity</param>
    /// <returns>Updated cart</returns>
    /// <response code="200">Item added successfully</response>
    /// <response code="400">If request is invalid or product not found</response>
    [HttpPost("add")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Add to cart", Description = "Add a product variant to the cart")]
    public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Auto-detect customerId from token if authenticated and not provided
        if (!request.CustomerId.HasValue && User.Identity?.IsAuthenticated == true)
        {
            request.CustomerId = User.GetCustomerId();
        }

        try
        {
            var cart = await _cartService.AddToCartAsync(request);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    /// <param name="request">Update request containing cart item ID and new quantity</param>
    /// <returns>Updated cart</returns>
    /// <response code="200">Quantity updated successfully</response>
    /// <response code="400">If request is invalid</response>
    /// <response code="404">If cart item not found</response>
    [HttpPut("update")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Update cart item", Description = "Update the quantity of an item in the cart")]
    public async Task<ActionResult<CartDto>> UpdateCartItem([FromBody] UpdateCartItemRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var cart = await _cartService.UpdateCartItemAsync(request);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    /// <param name="cartItemId">Cart item ID to remove</param>
    /// <response code="204">Item removed successfully</response>
    /// <response code="400">If cart item not found</response>
    [HttpDelete("remove/{cartItemId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Remove cart item", Description = "Remove an item from the cart")]
    public async Task<IActionResult> RemoveCartItem(int cartItemId)
    {
        try
        {
            await _cartService.RemoveCartItemAsync(cartItemId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Clear all items from cart
    /// </summary>
    /// <param name="cartId">Cart ID to clear</param>
    /// <response code="204">Cart cleared successfully</response>
    /// <response code="400">If cart not found</response>
    [HttpDelete("clear/{cartId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Clear cart", Description = "Remove all items from the cart")]
    public async Task<IActionResult> ClearCart(int cartId)
    {
        try
        {
            await _cartService.ClearCartAsync(cartId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Merge guest cart with customer cart (BR-006)
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="sessionId">Guest session ID</param>
    /// <returns>Merged cart</returns>
    /// <response code="200">Carts merged successfully</response>
    /// <response code="400">If sessionId is missing or merge fails</response>
    [HttpPost("merge")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Merge carts", Description = "Merge guest cart with customer cart on login (BR-006)")]
    public async Task<ActionResult<CartDto>> MergeCarts([FromQuery] int customerId, [FromQuery] string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return BadRequest("SessionId is required");
        }

        try
        {
            var cart = await _cartService.MergeCartsAsync(customerId, sessionId);
            return Ok(cart);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
