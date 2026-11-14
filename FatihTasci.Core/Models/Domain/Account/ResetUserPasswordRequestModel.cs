using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Account;

public class ResetUserPasswordRequestModel : IRequestModel
{
    public string Password { get; set; }
}

public class ResetUserPasswordRequestModelValidator : AbstractValidator<ResetUserPasswordRequestModel>
{
    public ResetUserPasswordRequestModelValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Şifre en fazla 100 karakter olabilir.");
    }
}
