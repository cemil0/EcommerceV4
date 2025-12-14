using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Api.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Geçerli bir müşteri ID'si gereklidir");

        RuleFor(x => x.ShippingAddressId)
            .GreaterThan(0).WithMessage("Teslimat adresi seçilmelidir");

        RuleFor(x => x.BillingAddressId)
            .GreaterThan(0).WithMessage("Fatura adresi seçilmelidir");

        RuleFor(x => x.OrderType)
            .NotEmpty().WithMessage("Sipariş tipi belirtilmelidir")
            .Must(x => new[] { "B2C", "B2B" }.Contains(x))
            .WithMessage("Sipariş tipi B2C veya B2B olmalıdır");

        RuleFor(x => x.CustomerNotes)
            .MaximumLength(500).WithMessage("Sipariş notu en fazla 500 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.CustomerNotes));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Sipariş en az bir ürün içermelidir")
            .Must(items => items.Count <= 50).WithMessage("Bir siparişte en fazla 50 ürün olabilir");

        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemRequestValidator());
    }
}

public class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .GreaterThan(0).WithMessage("Geçerli bir ürün seçilmelidir");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar en az 1 olmalıdır")
            .LessThanOrEqualTo(100).WithMessage("Bir üründen en fazla 100 adet sipariş edilebilir");
    }
}
