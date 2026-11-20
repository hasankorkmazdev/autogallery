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
    /// Aracın satış danışmanı: aktif bir panel kullanıcısı (zorunlu). Sitede "Ara" ve WhatsApp onun telefonuna gider.
    /// </summary>
    public Guid? AdvisorId { get; set; }

    public bool IsFeatured { get; set; }
    public int FeaturedOrder { get; set; }

    /// <summary>
    /// Ana sayfadaki araç kaydırıcısının "Fırsat Araçları" sekmesinde gösterilir.
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
        // HTML uzunluğu (biçimlendirme dahil); panelde schemas.ts ile aynı.
        RuleFor(x => x.Description)
            .MaximumLength(20000).WithMessage("Açıklama biçimlendirmesiyle birlikte en fazla 20.000 karakter olabilir.");
        // Danışmanın var ve aktif olduğu VehicleService'te kontrol edilir.
        RuleFor(x => x.AdvisorId)
            .NotEmpty().WithMessage("Danışman seçin.");
        RuleFor(x => x.FeaturedOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Vitrin sırası 0 ya da daha büyük olmalıdır.");
    }
}
