using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;
    private const string SessionCartKey = "CartSessionId";

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    // GET: /Cart
    public async Task<IActionResult> Index()
    {
        var sessionId = GetOrCreateSessionId();
        var cart = await _cartService.GetCartAsync(null, sessionId);

        return View(cart);
    }

    // POST: /Cart/AddToCart
    [HttpPost]
    public async Task<IActionResult> AddToCart(int productVariantId, int quantity = 1)
    {
        var sessionId = GetOrCreateSessionId();

        var request = new AddToCartRequest
        {
            CustomerId = null, // TODO: Get from authenticated user
            SessionId = sessionId,
            ProductVariantId = productVariantId,
            Quantity = quantity
        };

        try
        {
            await _cartService.AddToCartAsync(request);
            TempData["Success"] = "Ürün sepete eklendi!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Hata: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Cart/UpdateQuantity
    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        if (quantity <= 0)
        {
            return await RemoveItem(cartItemId);
        }

        try
        {
            var request = new UpdateCartItemRequest
            {
                CartItemId = cartItemId,
                Quantity = quantity
            };

            await _cartService.UpdateCartItemAsync(request);
            TempData["Success"] = "Sepet güncellendi!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Hata: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Cart/RemoveItem
    [HttpPost]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        try
        {
            await _cartService.RemoveCartItemAsync(cartItemId);
            TempData["Success"] = "Ürün sepetten çıkarıldı!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Hata: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Cart/Clear
    [HttpPost]
    public async Task<IActionResult> Clear()
    {
        var sessionId = GetOrCreateSessionId();
        var cart = await _cartService.GetCartAsync(null, sessionId);

        if (cart != null)
        {
            await _cartService.ClearCartAsync(cart.CartId);
            TempData["Success"] = "Sepet temizlendi!";
        }

        return RedirectToAction(nameof(Index));
    }

    // Helper method to get or create session ID
    private string GetOrCreateSessionId()
    {
        var sessionId = HttpContext.Session.GetString(SessionCartKey);

        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
            HttpContext.Session.SetString(SessionCartKey, sessionId);
        }

        return sessionId;
    }

    // TODO: Call this method when user logs in (BR-006: Cart Merging)
    public async Task<IActionResult> MergeGuestCart(int customerId)
    {
        var sessionId = HttpContext.Session.GetString(SessionCartKey);

        if (!string.IsNullOrEmpty(sessionId))
        {
            try
            {
                await _cartService.MergeCartsAsync(customerId, sessionId);
                HttpContext.Session.Remove(SessionCartKey); // Clear session after merge
                TempData["Success"] = "Sepetiniz birleştirildi!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Sepet birleştirme hatası: {ex.Message}";
            }
        }

        return RedirectToAction(nameof(Index));
    }
}
