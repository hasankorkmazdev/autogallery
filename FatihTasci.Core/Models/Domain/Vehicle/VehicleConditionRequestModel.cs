using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Vehicle;

// Kurallar panelin zod şemalarıyla (FatihTasci.Admin/src/pages/vehicles/wizard/schemas.ts) aynıdır.
// Adımlar arası kurallar (sıfır araçta km, yayın için fiyat) VehicleService'te kontrol edilir.

/// <summary>
/// Adım 3: Durum
/// </summary>
public class VehicleConditionRequestModel : IRequestModel
{
    public VehicleCondition Condition { get; set; } = VehicleCondition.Used;
    public bool HasWarranty { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public bool HeavyDamageRecord { get; set; }
    public PlateOrigin PlateOrigin { get; set; } = PlateOrigin.Turkish;
    public bool AcceptsTrade { get; set; }
    public decimal? TramerAmount { get; set; }
}

public class VehicleConditionRequestModelValidator : AbstractValidator<VehicleConditionRequestModel>
{
    public VehicleConditionRequestModelValidator()
    {
        RuleFor(x => x.Condition).IsInEnum().WithMessage("Araç durumu seçin.");
        RuleFor(x => x.PlateOrigin).IsInEnum().WithMessage("Plaka / uyruk seçin.");

        // Garanti açıksa bitiş tarihi zorunlu ve bugünden sonra olmalı.
        When(x => x.HasWarranty, () =>
        {
            RuleFor(x => x.WarrantyEndDate)
                .NotNull().WithMessage("Garanti bitiş tarihi zorunludur.")
                .Must(x => x.Value.Date > DateTime.Today).When(x => x.WarrantyEndDate.HasValue)
                .WithMessage("Garanti bitiş tarihi bugünden sonra olmalıdır.");
        });

        RuleFor(x => x.TramerAmount)
            .InclusiveBetween(0, 100_000_000).When(x => x.TramerAmount.HasValue)
            .WithMessage("Tramer tutarı 0 ya da daha büyük olmalıdır.");
    }
}
