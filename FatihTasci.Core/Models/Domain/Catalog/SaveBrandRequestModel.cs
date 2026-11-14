using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Catalog;

public class SaveBrandRequestModel : IRequestModel
{
    public string Name { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SaveBrandRequestModelValidator : AbstractValidator<SaveBrandRequestModel>
{
    public SaveBrandRequestModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Marka adı zorunludur.")
            .MaximumLength(60).WithMessage("Marka adı en fazla 60 karakter olabilir.");
        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sıra 0 ya da daha büyük olmalıdır.");
    }
}
