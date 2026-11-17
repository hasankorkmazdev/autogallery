using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Content;

/// <summary>
/// Vitrin yazıları: Hizmetler, Kurumsal ve "Sayılarla Biz". Panelde Site İçeriği sayfasından düzenlenir.
/// Hiç kaydedilmemiş alan için SiteContentDefaults'taki metin gelir; boş kaydedilen metin ve boş rakam listesi
/// vitrinde ilgili bölümü gizler.
/// </summary>
public class SiteContentModel : IRequestModel
{
    public const int MaxStats = 6;
    public const int MaxCorporateIntroLength = 500;
    public const int MaxCorporateAboutLength = 3000;
    public const int MaxCorporateStatementLength = 1000;

    /// <summary>
    /// SiteServiceType sırasıyla, her türden bir tane.
    /// </summary>
    public List<ServiceContentModel> Services { get; set; } = new();

    /// <summary>
    /// Kurumsal sayfanın giriş paragrafı.
    /// </summary>
    public string CorporateIntro { get; set; }

    /// <summary>
    /// Kurumsal sayfanın "Hakkımızda" metni.
    /// </summary>
    public string CorporateAbout { get; set; }

    public string CorporateVision { get; set; }
    public string CorporateMission { get; set; }

    /// <summary>
    /// "Sayılarla Biz" rakamları (sıralı). Ana sayfada ve Kurumsal sayfada görünür.
    /// </summary>
    public List<StatItemModel> Stats { get; set; } = new();

    /// <summary>
    /// Ana sayfa girişinin videosu. Yalnız okunur (bkz. HeroVideoModel).
    /// </summary>
    public HeroVideoModel HeroVideo { get; set; }
}
public class SiteContentModelValidator : AbstractValidator<SiteContentModel>
{
    public SiteContentModelValidator()
    {
        RuleFor(x => x.Services)
            .NotNull().WithMessage("Hizmet yazıları eksik.")
            .Must(x => x.Select(s => s.Type).Distinct().Count() == x.Count).When(x => x.Services != null)
            .WithMessage("Her hizmet bir kez gönderilmelidir.");
        RuleForEach(x => x.Services).SetValidator(new ServiceContentModelValidator());
        RuleFor(x => x.CorporateIntro)
            .MaximumLength(SiteContentModel.MaxCorporateIntroLength)
            .WithMessage($"Giriş yazısı en fazla {SiteContentModel.MaxCorporateIntroLength} karakter olabilir.");
        RuleFor(x => x.CorporateAbout)
            .MaximumLength(SiteContentModel.MaxCorporateAboutLength)
            .WithMessage($"Hakkımızda metni en fazla {SiteContentModel.MaxCorporateAboutLength} karakter olabilir.");
        RuleFor(x => x.CorporateVision)
            .MaximumLength(SiteContentModel.MaxCorporateStatementLength)
            .WithMessage($"Vizyon metni en fazla {SiteContentModel.MaxCorporateStatementLength} karakter olabilir.");
        RuleFor(x => x.CorporateMission)
            .MaximumLength(SiteContentModel.MaxCorporateStatementLength)
            .WithMessage($"Misyon metni en fazla {SiteContentModel.MaxCorporateStatementLength} karakter olabilir.");
        RuleFor(x => x.Stats)
            .Must(x => x == null || x.Count <= SiteContentModel.MaxStats)
            .WithMessage($"En fazla {SiteContentModel.MaxStats} rakam eklenebilir.");
        RuleForEach(x => x.Stats).SetValidator(new StatItemModelValidator());
    }
}
