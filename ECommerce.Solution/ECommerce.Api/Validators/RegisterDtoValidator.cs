using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Api.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi zorunludur")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz")
            .MaximumLength(100).WithMessage("E-posta adresi en fazla 100 karakter olabilir");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır")
            .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir")
            .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir")
            .Matches(@"[0-9]").WithMessage("Şifre en az bir rakam içermelidir")
            .Matches(@"[\!\@\#\$\%\^\&\*\(\)\_\+\-\=\[\]\{\}\;\:\'\,\.\<\>\?]")
            .WithMessage("Şifre en az bir özel karakter içermelidir");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Şifreler eşleşmiyor");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad zorunludur")
            .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir")
            .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Ad sadece harf içerebilir");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad zorunludur")
            .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir")
            .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Soyad sadece harf içerebilir");

        RuleFor(x => x.Phone)
            .Matches(@"^(\+90|0)?[0-9]{10}$")
            .WithMessage("Geçerli bir telefon numarası giriniz (örn: 05551234567)")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}
