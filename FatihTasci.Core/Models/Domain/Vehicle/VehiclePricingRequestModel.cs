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
    public bool IsFeatured { get; set; }
    public int FeaturedOrder { get; set; }
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
        RuleFor(x => x.FeaturedOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Vitrin sırası 0 ya da daha büyük olmalıdır.");
    }
}
