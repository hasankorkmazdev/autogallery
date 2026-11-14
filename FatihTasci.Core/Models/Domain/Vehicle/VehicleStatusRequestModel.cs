using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Vehicle;

// Kurallar panelin zod şemalarıyla (FatihTasci.Admin/src/pages/vehicles/wizard/schemas.ts) aynıdır.
// Adımlar arası kurallar (sıfır araçta km, yayın için fiyat) VehicleService'te kontrol edilir.

public class VehicleStatusRequestModel : IRequestModel
{
    public VehicleStatus Status { get; set; }
}

public class VehicleStatusRequestModelValidator : AbstractValidator<VehicleStatusRequestModel>
{
    public VehicleStatusRequestModelValidator()
    {
        RuleFor(x => x.Status).IsInEnum().WithMessage("Durum geçersiz.");
    }
}
