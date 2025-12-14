using ECommerce.Application.DTOs.Common;
using FluentValidation;

namespace ECommerce.Api.Validators;

public class PagedRequestValidator : AbstractValidator<PagedRequest>
{
    public PagedRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 1'den büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 1'den büyük olmalıdır")
            .LessThanOrEqualTo(100).WithMessage("Sayfa boyutu en fazla 100 olabilir");

        RuleFor(x => x.SortBy)
            .MaximumLength(50).WithMessage("Sıralama alanı en fazla 50 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.SortBy));
    }
}
