using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Api.Validators;

public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        RuleFor(x => x.CartItemId)
            .GreaterThan(0).WithMessage("Geçerli bir sepet öğesi seçilmelidir");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar en az 1 olmalıdır")
            .LessThanOrEqualTo(100).WithMessage("Miktar en fazla 100 olabilir");
    }
}
