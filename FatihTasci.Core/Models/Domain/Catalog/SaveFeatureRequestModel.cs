using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Catalog;

public class SaveFeatureRequestModel : IRequestModel
{
    public FeatureCategory Category { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SaveFeatureRequestModelValidator : AbstractValidator<SaveFeatureRequestModel>
{
    public SaveFeatureRequestModelValidator()
    {
        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Kategori seçin.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Donanım adı zorunludur.")
            .MaximumLength(80).WithMessage("Donanım adı en fazla 80 karakter olabilir.");
        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sıra 0 ya da daha büyük olmalıdır.");
    }
}
