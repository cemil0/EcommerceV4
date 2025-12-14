using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Api.Validators;

public class AddToCartRequestValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartRequestValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .GreaterThan(0).WithMessage("Geçerli bir ürün seçilmelidir");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar en az 1 olmalıdır")
            .LessThanOrEqualTo(100).WithMessage("Tek seferde en fazla 100 adet eklenebilir");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Geçerli bir müşteri ID'si gereklidir")
            .When(x => x.CustomerId.HasValue);
    }
}
