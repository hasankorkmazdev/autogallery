using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Catalog;

public class SaveSeriesRequestModel : IRequestModel
{
    public int BrandId { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SaveSeriesRequestModelValidator : AbstractValidator<SaveSeriesRequestModel>
{
    public SaveSeriesRequestModelValidator()
    {
        RuleFor(x => x.BrandId)
            .GreaterThan(0).WithMessage("Marka seçin.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Seri adı zorunludur.")
            .MaximumLength(80).WithMessage("Seri adı en fazla 80 karakter olabilir.");
        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sıra 0 ya da daha büyük olmalıdır.");
    }
}
