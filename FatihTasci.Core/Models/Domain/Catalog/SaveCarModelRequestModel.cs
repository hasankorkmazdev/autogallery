using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Catalog;

public class SaveCarModelRequestModel : IRequestModel
{
    public int SeriesId { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SaveCarModelRequestModelValidator : AbstractValidator<SaveCarModelRequestModel>
{
    public SaveCarModelRequestModelValidator()
    {
        RuleFor(x => x.SeriesId)
            .GreaterThan(0).WithMessage("Seri seçin.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Model adı zorunludur.")
            .MaximumLength(120).WithMessage("Model adı en fazla 120 karakter olabilir.");
    }
}
