using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Api.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi zorunludur")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur");
    }
}
