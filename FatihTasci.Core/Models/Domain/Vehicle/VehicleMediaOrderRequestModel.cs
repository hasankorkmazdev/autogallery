using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Vehicle;

// Kurallar panelin zod şemalarıyla (FatihTasci.Admin/src/pages/vehicles/wizard/schemas.ts) aynıdır.
// Adımlar arası kurallar (sıfır araçta km, yayın için fiyat) VehicleService'te kontrol edilir.

/// <summary>
/// Adım 6: Fotoğraf sıralaması
/// </summary>
public class VehicleMediaOrderRequestModel : IRequestModel
{
    public List<Guid> MediaIds { get; set; } = new();
}

public class VehicleMediaOrderRequestModelValidator : AbstractValidator<VehicleMediaOrderRequestModel>
{
    public VehicleMediaOrderRequestModelValidator()
    {
        RuleFor(x => x.MediaIds)
            .NotEmpty().WithMessage("Sıralanacak fotoğraf yok.")
            .Must(x => x.Distinct().Count() == x.Count).WithMessage("Aynı fotoğraf birden fazla kez gönderilemez.");
    }
}
