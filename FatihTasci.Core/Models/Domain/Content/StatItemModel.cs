using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Content;

/// <summary>
/// "Sayılarla Biz" bölümündeki bir rakam, örn. "40" / "Yıllık Tecrübe". Rakam yazı olarak saklanır ("5.000+").
/// </summary>
public class StatItemModel
{
    public const int MaxValueLength = 12;
    public const int MaxLabelLength = 40;

    public string Value { get; set; }
    public string Label { get; set; }
}
public class StatItemModelValidator : AbstractValidator<StatItemModel>
{
    public StatItemModelValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Rakam zorunludur.")
            .MaximumLength(StatItemModel.MaxValueLength).WithMessage($"Rakam en fazla {StatItemModel.MaxValueLength} karakter olabilir.");
        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Açıklama zorunludur.")
            .MaximumLength(StatItemModel.MaxLabelLength).WithMessage($"Açıklama en fazla {StatItemModel.MaxLabelLength} karakter olabilir.");
    }
}
