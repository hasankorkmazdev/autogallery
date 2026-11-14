using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Account;

public class LoginRequestModel : IRequestModel
{
    public string EMail { get; set; }
    public string Password { get; set; }
}

public class LoginRequestModelValidator : AbstractValidator<LoginRequestModel>
{
    public LoginRequestModelValidator()
    {
        RuleFor(x => x.EMail)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi girin.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.");
    }
}
