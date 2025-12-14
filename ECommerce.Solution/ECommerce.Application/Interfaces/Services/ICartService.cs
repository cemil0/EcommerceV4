using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces.Services;

public interface ICartService
{
    Task<CartDto?> GetCartAsync(int? customerId, string? sessionId);
    Task<CartDto> AddToCartAsync(AddToCartRequest request);
    Task<CartDto> UpdateCartItemAsync(UpdateCartItemRequest request);
    Task RemoveCartItemAsync(int cartItemId);
    Task ClearCartAsync(int cartId);
    Task<CartDto> MergeCartsAsync(int customerId, string sessionId); // BR-006
}
