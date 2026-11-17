using FatihTasci.Core.Enums;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Setting;

/// <summary>
/// Ayarlardaki bir sosyal medya / mağaza bağlantısı. Liste sırası vitrindeki sıradır.
/// </summary>
public class SocialLinkModel
{
    public SocialPlatform Platform { get; set; }
    public string Url { get; set; }
}
public class SocialLinkModelValidator : AbstractValidator<SocialLinkModel>
{
    public SocialLinkModelValidator()
    {
        RuleFor(x => x.Platform).IsInEnum().WithMessage("Platform seçin.");
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Bağlantı zorunludur.")
            .MaximumLength(300).WithMessage("Bağlantı en fazla 300 karakter olabilir.")
            .Must(SiteSettingsModelValidator.BeHttpsUrl).When(x => !string.IsNullOrWhiteSpace(x.Url))
            .WithMessage("Geçerli bir https bağlantısı girin.");
    }
}
