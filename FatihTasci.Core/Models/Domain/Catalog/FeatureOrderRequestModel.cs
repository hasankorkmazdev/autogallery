using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Catalog;

/// <summary>
/// Bir kategorideki donanımların yeni sırası (pasifler dahil, hepsi).
/// </summary>
public class FeatureOrderRequestModel : IRequestModel
{
    public FeatureCategory Category { get; set; }
    public List<int> FeatureIds { get; set; } = new();
}

public class FeatureOrderRequestModelValidator : AbstractValidator<FeatureOrderRequestModel>
{
    public FeatureOrderRequestModelValidator()
    {
        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Kategori seçin.");
        RuleFor(x => x.FeatureIds)
            .NotEmpty().WithMessage("Sıralanacak donanım yok.")
            .Must(x => x.Distinct().Count() == x.Count).WithMessage("Aynı donanım birden fazla kez gönderilemez.");
    }
}
