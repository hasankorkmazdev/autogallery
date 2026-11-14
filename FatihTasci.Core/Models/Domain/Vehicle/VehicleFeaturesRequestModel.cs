using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Vehicle;

// Kurallar panelin zod şemalarıyla (FatihTasci.Admin/src/pages/vehicles/wizard/schemas.ts) aynıdır.
// Adımlar arası kurallar (sıfır araçta km, yayın için fiyat) VehicleService'te kontrol edilir.

/// <summary>
/// Adım 5: Donanım
/// </summary>
public class VehicleFeaturesRequestModel : IRequestModel
{
    public List<int> FeatureIds { get; set; } = new();
}

public class VehicleFeaturesRequestModelValidator : AbstractValidator<VehicleFeaturesRequestModel>
{
    public VehicleFeaturesRequestModelValidator()
    {
        RuleFor(x => x.FeatureIds).NotNull().WithMessage("Donanım listesi gönderilmelidir.");
    }
}
