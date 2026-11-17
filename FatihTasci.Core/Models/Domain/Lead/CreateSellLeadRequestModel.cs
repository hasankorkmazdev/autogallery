using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace FatihTasci.Core.Models.Domain.Lead;

/// <summary>
/// "Aracımı Satmak İstiyorum" sayfasındaki fotoğraflı teklif formu (LeadType.SellOrTrade).
/// Marka / Model / Paket serbest metindir. Kilometre ve fiyat metin olarak gelir; ziyaretçi "125.000" gibi
/// ayraçlı yazabilir, sayıya SellKmValue / SellExpectedPriceValue ile çevrilir.
/// Fotoğraflar isteğe bağlıdır; vitrin (site.js) göndermeden önce küçültür, sınırlar yine sunucuda kontrol edilir.
/// </summary>
public class CreateSellLeadRequestModel : IRequestModel
{
    public const int MaxPhotos = 8;
    public const long MaxPhotoBytes = 15 * 1024 * 1024;
    public const int MinYear = 1950;
    public const int MaxKm = 2_000_000;
    public const long MaxPrice = 1_000_000_000;
    public static readonly string[] AllowedPhotoExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    public string SellBrand { get; set; }
    public string SellModel { get; set; }
    public string SellPackage { get; set; }
    public int? SellYear { get; set; }
    public string SellKm { get; set; }
    public SellDamageState? SellDamage { get; set; }
    public string SellExpectedPrice { get; set; }

    /// <summary>
    /// Hasar, tramer ve eklemek istedikleri (talebin Message alanına yazılır)
    /// </summary>
    public string Message { get; set; }

    public string FullName { get; set; }
    public string Phone { get; set; }
    public bool KvkkConsent { get; set; }

    /// <summary>
    /// Bot tuzağı: gizli alan, insanlar boş bırakır.
    /// </summary>
    public string Website { get; set; }

    public List<IFormFile> Photos { get; set; } = new();

    public int? SellKmValue => ParseWholeNumber(SellKm) is { } km && km <= MaxKm ? (int)km : null;

    public decimal? SellExpectedPriceValue => ParseWholeNumber(SellExpectedPrice) is { } price && price > 0 && price <= MaxPrice ? price : null;

    /// <summary>
    /// Boşluk, nokta ve virgül ayraçlarını atıp tam sayıya çevirir ("125.000" → 125000). Geçersizse null.
    /// </summary>
    public static long? ParseWholeNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var digits = new string(value.Where(c => !char.IsWhiteSpace(c) && c != '.' && c != ',').ToArray());
        if (digits.Length == 0 || digits.Length > 12 || !digits.All(char.IsAsciiDigit))
            return null;

        return long.Parse(digits);
    }
}

public class CreateSellLeadRequestModelValidator : AbstractValidator<CreateSellLeadRequestModel>
{
    public CreateSellLeadRequestModelValidator()
    {
        RuleFor(x => x.SellBrand)
            .NotEmpty().WithMessage("Marka zorunludur.")
            .MaximumLength(60).WithMessage("Marka en fazla 60 karakter olabilir.");
        RuleFor(x => x.SellModel)
            .NotEmpty().WithMessage("Model zorunludur.")
            .MaximumLength(120).WithMessage("Model en fazla 120 karakter olabilir.");
        RuleFor(x => x.SellPackage)
            .MaximumLength(120).WithMessage("Paket en fazla 120 karakter olabilir.");
        RuleFor(x => x.SellYear)
            .NotNull().WithMessage("Model yılını seçin.")
            .InclusiveBetween(CreateSellLeadRequestModel.MinYear, DateTime.Today.Year + 1).WithMessage("Model yılı geçersiz.");
        RuleFor(x => x.SellKm)
            .NotEmpty().WithMessage("Kilometre zorunludur.");
        RuleFor(x => x.SellKmValue)
            .NotNull().When(x => !string.IsNullOrWhiteSpace(x.SellKm))
            .OverridePropertyName(nameof(CreateSellLeadRequestModel.SellKm))
            .WithMessage("Geçerli bir kilometre girin.");
        RuleFor(x => x.SellDamage)
            .NotNull().WithMessage("Hasar durumunu seçin.")
            .IsInEnum().WithMessage("Hasar durumu geçersiz.");
        RuleFor(x => x.SellExpectedPriceValue)
            .NotNull().When(x => !string.IsNullOrWhiteSpace(x.SellExpectedPrice))
            .OverridePropertyName(nameof(CreateSellLeadRequestModel.SellExpectedPrice))
            .WithMessage("Geçerli bir fiyat girin.");
        RuleFor(x => x.Message)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad soyad zorunludur.")
            .MaximumLength(100).WithMessage("Ad soyad en fazla 100 karakter olabilir.");
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon zorunludur.")
            .Matches(@"^[0-9 +()\-]{10,20}$").WithMessage("Geçerli bir telefon numarası girin.");
        RuleFor(x => x.KvkkConsent)
            .Equal(true).WithMessage("Devam etmek için aydınlatma metnini onaylayın.");

        // Tüm fotoğraf hataları "Photos" alanında gösterilir (formda tek hata satırı).
        RuleFor(x => x.Photos).Custom((photos, context) =>
        {
            if (photos == null || photos.Count == 0)
                return;

            if (photos.Count > CreateSellLeadRequestModel.MaxPhotos)
            {
                context.AddFailure(nameof(CreateSellLeadRequestModel.Photos), $"En fazla {CreateSellLeadRequestModel.MaxPhotos} fotoğraf yükleyebilirsiniz.");
                return;
            }

            foreach (var photo in photos)
            {
                var name = Path.GetFileName(photo.FileName);
                if (!CreateSellLeadRequestModel.AllowedPhotoExtensions.Contains(Path.GetExtension(name).ToLowerInvariant()))
                    context.AddFailure(nameof(CreateSellLeadRequestModel.Photos), $"{name}: yalnızca JPG, PNG ve WebP fotoğraf yüklenebilir.");
                else if (photo.Length == 0 || photo.Length > CreateSellLeadRequestModel.MaxPhotoBytes)
                    context.AddFailure(nameof(CreateSellLeadRequestModel.Photos), $"{name}: fotoğraf en fazla {CreateSellLeadRequestModel.MaxPhotoBytes / 1024 / 1024} MB olabilir.");
            }
        });
    }
}
