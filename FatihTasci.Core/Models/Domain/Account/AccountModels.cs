using FatihTasci.Core.Enums;
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

public class UserModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string EMail { get; set; }
    public UserRole Role { get; set; }
}

public class LoginResponseModel
{
    public UserModel User { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}
