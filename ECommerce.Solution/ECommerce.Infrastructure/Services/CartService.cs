using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CartService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CartDto?> GetCartAsync(int? customerId, string? sessionId)
    {
        Cart? cart = null;

        if (customerId.HasValue)
        {
            cart = await _unitOfWork.Carts
                .Query()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            cart = await _unitOfWork.Carts
                .Query()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
        }

        return cart != null ? _mapper.Map<CartDto>(cart) : null;
    }

    public async Task<CartDto> AddToCartAsync(AddToCartRequest request)
    {
        // Get or create cart
        Cart? cart = null;

        if (request.CustomerId.HasValue)
        {
            cart = await _unitOfWork.Carts.GetByCustomerIdAsync(request.CustomerId.Value);
        }
        else if (!string.IsNullOrEmpty(request.SessionId))
        {
            cart = await _unitOfWork.Carts.GetBySessionIdAsync(request.SessionId);
        }

        if (cart == null)
        {
            cart = new Cart
            {
                CustomerId = request.CustomerId,
                SessionId = request.SessionId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Carts.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();
        }

        // Check if item already exists in cart
        var existingItem = await _unitOfWork.CartItems
            .GetByCartAndVariantAsync(cart.CartId, request.ProductVariantId);

        if (existingItem != null)
        {
            // Update quantity (BR-006: Merge logic)
            existingItem.Quantity += request.Quantity;
            existingItem.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.CartItems.Update(existingItem);
        }
        else
        {
            // Get product variant for pricing
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(request.ProductVariantId);
            if (variant == null)
                throw new InvalidOperationException("Product variant not found");

            // Add new item
            var cartItem = new CartItem
            {
                CartId = cart.CartId,
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                UnitPrice = variant.SalePrice ?? variant.BasePrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.CartItems.AddAsync(cartItem);
        }

        await _unitOfWork.SaveChangesAsync();

        // Reload cart with items
        var updatedCart = await _unitOfWork.Carts
            .Query()
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.Product)
            .FirstOrDefaultAsync(c => c.CartId == cart.CartId);
            
        return _mapper.Map<CartDto>(updatedCart!);
    }

    public async Task<CartDto> UpdateCartItemAsync(UpdateCartItemRequest request)
    {
        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(request.CartItemId);
        if (cartItem == null)
            throw new InvalidOperationException("Cart item not found");

        cartItem.Quantity = request.Quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.CartItems.Update(cartItem);

        await _unitOfWork.SaveChangesAsync();

        var cart = await _unitOfWork.Carts
            .Query()
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.Product)
            .FirstOrDefaultAsync(c => c.CartId == cartItem.CartId);
            
        return _mapper.Map<CartDto>(cart!);
    }

    public async Task RemoveCartItemAsync(int cartItemId)
    {
        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(cartItemId);
        if (cartItem != null)
        {
            _unitOfWork.CartItems.Remove(cartItem);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(int cartId)
    {
        var cart = await _unitOfWork.Carts.GetActiveCartWithItemsAsync(cartId);
        if (cart != null && cart.CartItems.Any())
        {
            _unitOfWork.CartItems.RemoveRange(cart.CartItems);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<CartDto> MergeCartsAsync(int customerId, string sessionId)
    {
        // BR-006: Cart Merging Logic
        var customerCart = await _unitOfWork.Carts.GetByCustomerIdAsync(customerId);
        var sessionCart = await _unitOfWork.Carts.GetBySessionIdAsync(sessionId);

        if (sessionCart == null)
        {
            // No session cart, return customer cart
            var cart = customerCart ?? new Cart
            {
                CustomerId = customerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (customerCart == null)
            {
                await _unitOfWork.Carts.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            }

            return _mapper.Map<CartDto>(cart);
        }

        if (customerCart == null)
        {
            // No customer cart, assign session cart to customer
            sessionCart.CustomerId = customerId;
            sessionCart.SessionId = null;
            sessionCart.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Carts.Update(sessionCart);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CartDto>(sessionCart);
        }

        // Both carts exist, merge them
        var sessionCartWithItems = await _unitOfWork.Carts.GetActiveCartWithItemsAsync(sessionCart.CartId);
        if (sessionCartWithItems != null && sessionCartWithItems.CartItems.Any())
        {
            foreach (var sessionItem in sessionCartWithItems.CartItems)
            {
                var existingItem = await _unitOfWork.CartItems
                    .GetByCartAndVariantAsync(customerCart.CartId, sessionItem.ProductVariantId);

                if (existingItem != null)
                {
                    // Merge quantities
                    existingItem.Quantity += sessionItem.Quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.CartItems.Update(existingItem);
                }
                else
                {
                    // Move item to customer cart
                    sessionItem.CartId = customerCart.CartId;
                    sessionItem.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.CartItems.Update(sessionItem);
                }
            }
        }

        // Delete session cart
        _unitOfWork.Carts.Remove(sessionCart);
        await _unitOfWork.SaveChangesAsync();

        // Reload customer cart
        var mergedCart = await _unitOfWork.Carts.GetActiveCartWithItemsAsync(customerCart.CartId);
        return _mapper.Map<CartDto>(mergedCart!);
    }
}
