using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private const string SessionCartKey = "CartSessionId";

    public OrderController(IOrderService orderService, ICartService cartService)
    {
        _orderService = orderService;
        _cartService = cartService;
    }

    // GET: /Order
    public async Task<IActionResult> Index()
    {
        // TODO: Get from authenticated user
        int? customerId = null;

        if (customerId.HasValue)
        {
            var orders = await _orderService.GetByCustomerIdAsync(customerId.Value);
            return View(orders);
        }

        return View(new List<OrderDto>());
    }

    // GET: /Order/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderService.GetByIdAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    // GET: /Order/Checkout
    public async Task<IActionResult> Checkout()
    {
        var sessionId = HttpContext.Session.GetString(SessionCartKey);
        var cart = await _cartService.GetCartAsync(null, sessionId);

        if (cart == null || !cart.CartItems.Any())
        {
            TempData["Error"] = "Sepetiniz boş!";
            return RedirectToAction("Index", "Cart");
        }

        // TODO: Load customer addresses for selection
        ViewBag.Cart = cart;
        return View();
    }

    // POST: /Order/CreateB2C
    [HttpPost]
    public async Task<IActionResult> CreateB2C(int customerId, int billingAddressId, int shippingAddressId, string? couponCode, string? customerNotes)
    {
        try
        {
            // Get cart items
            var sessionId = HttpContext.Session.GetString(SessionCartKey);
            var cart = await _cartService.GetCartAsync(null, sessionId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Sepetiniz boş!";
                return RedirectToAction("Index", "Cart");
            }

            // Create order request
            var request = new CreateOrderRequest
            {
                CustomerId = customerId,
                OrderType = "B2C",
                BillingAddressId = billingAddressId,
                ShippingAddressId = shippingAddressId,
                CouponCode = couponCode,
                CustomerNotes = customerNotes,
                Items = cart.CartItems.Select(ci => new CreateOrderItemRequest
                {
                    ProductVariantId = ci.ProductVariantId,
                    Quantity = ci.Quantity
                }).ToList()
            };

            var order = await _orderService.CreateB2COrderAsync(request);

            // Clear cart after successful order
            await _cartService.ClearCartAsync(cart.CartId);
            HttpContext.Session.Remove(SessionCartKey);

            TempData["Success"] = $"Siparişiniz oluşturuldu! Sipariş No: {order.OrderNumber}";
            return RedirectToAction(nameof(Details), new { id = order.OrderId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Sipariş oluşturulurken hata: {ex.Message}";
            return RedirectToAction(nameof(Checkout));
        }
    }

    // POST: /Order/CreateB2B
    [HttpPost]
    public async Task<IActionResult> CreateB2B(int customerId, int companyId, int billingAddressId, int shippingAddressId, string? customerNotes)
    {
        try
        {
            var sessionId = HttpContext.Session.GetString(SessionCartKey);
            var cart = await _cartService.GetCartAsync(null, sessionId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Sepetiniz boş!";
                return RedirectToAction("Index", "Cart");
            }

            var request = new CreateOrderRequest
            {
                CustomerId = customerId,
                CompanyId = companyId,
                OrderType = "B2B",
                BillingAddressId = billingAddressId,
                ShippingAddressId = shippingAddressId,
                CustomerNotes = customerNotes,
                Items = cart.CartItems.Select(ci => new CreateOrderItemRequest
                {
                    ProductVariantId = ci.ProductVariantId,
                    Quantity = ci.Quantity
                }).ToList()
            };

            var order = await _orderService.CreateB2BOrderAsync(request);

            await _cartService.ClearCartAsync(cart.CartId);
            HttpContext.Session.Remove(SessionCartKey);

            TempData["Success"] = $"B2B Siparişiniz oluşturuldu! Sipariş No: {order.OrderNumber}";
            return RedirectToAction(nameof(Details), new { id = order.OrderId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Sipariş oluşturulurken hata: {ex.Message}";
            return RedirectToAction(nameof(Checkout));
        }
    }
}
