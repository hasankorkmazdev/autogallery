using FatihTasci.Core.Models.Domain.Common;
using FatihTasci.Core.Providers;
using FluentValidation;
using System.Globalization;

namespace FatihTasci.Core.Models.Domain.Setting;

/// <summary>
/// Site ayarları. Her özellik SiteSettings tablosunda aynı adlı bir anahtar olarak saklanır.
/// </summary>
public class SiteSettingsModel : IRequestModel
{
    /// <summary>
    /// Firma (galeri) adı. Vitrin başlığı, sayfa başlıkları ve panelde kullanılır.
    /// </summary>
    public string GalleryName { get; set; }

    /// <summary>
    /// Firma logosu, public depoya göre göreli yol. PUT /api/settings ile değişmez; /api/settings/logo ile yüklenir.
    /// </summary>
    public string LogoPath { get; set; }

    /// <summary>
    /// Logo adresi (/media/...). Salt okunur, ayar olarak saklanmaz.
    /// </summary>
    public string LogoUrl => string.IsNullOrEmpty(LogoPath) ? null : $"{LocalMediaStorage.RequestPath}/{LogoPath}";

    /// <summary>
    /// Açıksa logo araç fotoğraflarının orta ve büyük varyantlarına filigran olarak basılır.
    /// </summary>
    public bool WatermarkEnabled { get; set; } = true;

    public string Phone { get; set; }

    /// <summary>
    /// Ülke koduyla, yalnızca rakam (örn. 905321234567). wa.me bağlantısında kullanılır.
    /// </summary>
    public string WhatsApp { get; set; }

    public string Email { get; set; }
    public string Address { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public string WorkingHours { get; set; }

    /// <summary>
    /// Sosyal medya ve ilan mağazası bağlantıları (sıralı). SiteSettings'te tek anahtarda JSON olarak saklanır.
    /// </summary>
    public List<SocialLinkModel> SocialLinks { get; set; } = new();
}
public class SiteSettingsModelValidator : AbstractValidator<SiteSettingsModel>
{
    public const int MaxSocialLinks = 12;

    public SiteSettingsModelValidator()
    {
        RuleFor(x => x.GalleryName)
            .NotEmpty().WithMessage("Galeri adı zorunludur.")
            .MaximumLength(80).WithMessage("Galeri adı en fazla 80 karakter olabilir.");
        RuleFor(x => x.Phone)
            .Matches(@"^[0-9 +()\-]{10,20}$").When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Geçerli bir telefon numarası girin.");
        RuleFor(x => x.WhatsApp)
            .Matches(@"^[0-9]{10,15}$").When(x => !string.IsNullOrWhiteSpace(x.WhatsApp))
            .WithMessage("WhatsApp numarasını ülke koduyla ve yalnızca rakamla girin (örn. 905321234567).");
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Geçerli bir e-posta adresi girin.");
        RuleFor(x => x.Address).MaximumLength(300).WithMessage("Adres en fazla 300 karakter olabilir.");
        RuleFor(x => x.Latitude)
            .Must(x => decimal.TryParse(x, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) && v >= -90 && v <= 90)
            .When(x => !string.IsNullOrWhiteSpace(x.Latitude))
            .WithMessage("Enlem -90 ile 90 arasında, nokta ile yazılmalıdır.");
        RuleFor(x => x.Longitude)
            .Must(x => decimal.TryParse(x, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) && v >= -180 && v <= 180)
            .When(x => !string.IsNullOrWhiteSpace(x.Longitude))
            .WithMessage("Boylam -180 ile 180 arasında, nokta ile yazılmalıdır.");
        RuleFor(x => x.WorkingHours).MaximumLength(200).WithMessage("Çalışma saatleri en fazla 200 karakter olabilir.");
        RuleFor(x => x.SocialLinks)
            .Must(x => x == null || x.Count <= MaxSocialLinks).WithMessage($"En fazla {MaxSocialLinks} bağlantı eklenebilir.");
        RuleForEach(x => x.SocialLinks).SetValidator(new SocialLinkModelValidator());    }

    public static bool BeHttpsUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
    }
}
