using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Vehicle;

// Kurallar panelin zod şemalarıyla (FatihTasci.Admin/src/pages/vehicles/wizard/schemas.ts) aynıdır.
// Adımlar arası kurallar (sıfır araçta km, yayın için fiyat) VehicleService'te kontrol edilir.

/// <summary>
/// Adım 7: Fiyat, açıklama ve vitrin
/// </summary>
public class VehiclePricingRequestModel : IRequestModel
{
    public decimal? Price { get; set; }

    /// <summary>
    /// Kampanyalı fiyat (boşsa kampanya yok). Fiyattan küçük olmalı.
    /// </summary>
    public decimal? CampaignPrice { get; set; }

    public bool ShowPrice { get; set; } = true;
    public string Description { get; set; }

    /// <summary>
    /// İlan yetkilisi (isteğe bağlı). Girilirse telefon da girilmelidir.
    /// </summary>
    public string ContactName { get; set; }

    /// <summary>
    /// İlanın iletişim telefonu (isteğe bağlı). Boşsa galeri telefonu kullanılır.
    /// </summary>
    public string ContactPhone { get; set; }

    public bool IsFeatured { get; set; }
    public int FeaturedOrder { get; set; }

    /// <summary>
    /// Ana sayfadaki "Fırsat Araçları" şeridinde gösterilir.
    /// </summary>
    public bool IsOpportunity { get; set; }
}

public class VehiclePricingRequestModelValidator : AbstractValidator<VehiclePricingRequestModel>
{
    public VehiclePricingRequestModelValidator()
    {
        RuleFor(x => x.Price)
            .InclusiveBetween(0, 1_000_000_000).When(x => x.Price.HasValue)
            .WithMessage("Fiyat 0 ile 1.000.000.000 TL arasında olmalıdır.");
        When(x => x.CampaignPrice.HasValue, () =>
        {
            RuleFor(x => x.CampaignPrice)
                .GreaterThan(0).WithMessage("Kampanyalı fiyat 0'dan büyük olmalıdır.")
                .Must((x, campaign) => x.Price > 0 && campaign < x.Price)
                .WithMessage("Kampanyalı fiyat, normal fiyattan düşük olmalıdır.");
        });
        RuleFor(x => x.Description)
            .MaximumLength(10000).WithMessage("Açıklama en fazla 10.000 karakter olabilir.");
        RuleFor(x => x.ContactName)
            .MaximumLength(80).WithMessage("Yetkili adı en fazla 80 karakter olabilir.");
        RuleFor(x => x.ContactPhone)
            .Matches(@"^[0-9 +()\-]{10,20}$").When(x => !string.IsNullOrWhiteSpace(x.ContactPhone))
            .WithMessage("Geçerli bir telefon numarası girin.");
        RuleFor(x => x.ContactPhone)
            .NotEmpty().When(x => !string.IsNullOrWhiteSpace(x.ContactName))
            .WithMessage("Yetkili adı girildiyse telefon da girilmelidir.");
        RuleFor(x => x.FeaturedOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Vitrin sırası 0 ya da daha büyük olmalıdır.");
    }
}
