namespace ECommerce.Application.DTOs;

public class CreateOrderRequest
{
    public int CustomerId { get; set; }
    public int? CompanyId { get; set; }
    public int CartId { get; set; } // Cart to convert to order
    public string OrderType { get; set; } = "B2C"; // B2C or B2B
    public int BillingAddressId { get; set; }
    public int ShippingAddressId { get; set; }
    public string? CouponCode { get; set; }
    public string? CustomerNotes { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = new(); // Optional: can be populated from cart
}

public class CreateOrderItemRequest
{
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Expected price from cart
}


public class AddToCartRequest
{
    public int? CustomerId { get; set; }
    public string? SessionId { get; set; }
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    public int CartItemId { get; set; }
    public int Quantity { get; set; }
}
