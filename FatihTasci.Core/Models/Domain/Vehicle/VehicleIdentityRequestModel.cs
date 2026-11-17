using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Vehicle;

// Kurallar panelin zod şemalarıyla (FatihTasci.Admin/src/pages/vehicles/wizard/schemas.ts) aynıdır.
// Adımlar arası kurallar (sıfır araçta km, yayın için fiyat) VehicleService'te kontrol edilir.

/// <summary>
/// Adım 1: Kimlik. POST /api/vehicles ile taslak oluştururken de kullanılır.
/// </summary>
public class VehicleIdentityRequestModel : IRequestModel
{
    public int BrandId { get; set; }
    public int SeriesId { get; set; }
    public int CarModelId { get; set; }
    public string Title { get; set; }
}

public class VehicleIdentityRequestModelValidator : AbstractValidator<VehicleIdentityRequestModel>
{
    public VehicleIdentityRequestModelValidator()
    {
        RuleFor(x => x.BrandId).GreaterThan(0).WithMessage("Marka seçin.");
        RuleFor(x => x.SeriesId).GreaterThan(0).WithMessage("Model seçin.");
        RuleFor(x => x.CarModelId).GreaterThan(0).WithMessage("Paket seçin.");
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("İlan başlığı zorunludur.")
            .MaximumLength(150).WithMessage("İlan başlığı en fazla 150 karakter olabilir.");
    }
}
