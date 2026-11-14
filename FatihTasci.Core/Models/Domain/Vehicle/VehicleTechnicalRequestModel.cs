using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Vehicle;

// Kurallar panelin zod şemalarıyla (FatihTasci.Admin/src/pages/vehicles/wizard/schemas.ts) aynıdır.
// Adımlar arası kurallar (sıfır araçta km, yayın için fiyat) VehicleService'te kontrol edilir.

/// <summary>
/// Adım 2: Teknik
/// </summary>
public class VehicleTechnicalRequestModel : IRequestModel
{
    public int? Year { get; set; }
    public int? Kilometer { get; set; }
    public FuelType? FuelType { get; set; }
    public TransmissionType? Transmission { get; set; }
    public BodyType? BodyType { get; set; }
    public int? EngineCc { get; set; }
    public int? PowerHp { get; set; }
    public Drivetrain? Drivetrain { get; set; }
    public int? BatteryKwh { get; set; }
    public int? RangeKm { get; set; }
    public int? ExteriorColorId { get; set; }
    public string InteriorColor { get; set; }
}

public class VehicleTechnicalRequestModelValidator : AbstractValidator<VehicleTechnicalRequestModel>
{
    public const int MinYear = 1950;
    public const int MaxKilometer = 2_000_000;

    public VehicleTechnicalRequestModelValidator()
    {
        var maxYear = DateTime.Now.Year + 1;

        RuleFor(x => x.Year)
            .NotNull().WithMessage("Model yılı zorunludur.")
            .InclusiveBetween(MinYear, maxYear).WithMessage($"Model yılı {MinYear} ile {maxYear} arasında olmalıdır.");
        RuleFor(x => x.Kilometer)
            .NotNull().WithMessage("Kilometre zorunludur.")
            .InclusiveBetween(0, MaxKilometer).WithMessage("Kilometre 0 ile 2.000.000 arasında olmalıdır.");
        RuleFor(x => x.FuelType)
            .NotNull().WithMessage("Yakıt tipi seçin.")
            .IsInEnum().WithMessage("Yakıt tipi geçersiz.");
        RuleFor(x => x.Transmission)
            .NotNull().WithMessage("Vites tipi seçin.")
            .IsInEnum().WithMessage("Vites tipi geçersiz.");
        RuleFor(x => x.BodyType)
            .NotNull().WithMessage("Kasa tipi seçin.")
            .IsInEnum().WithMessage("Kasa tipi geçersiz.");
        RuleFor(x => x.Drivetrain)
            .NotNull().WithMessage("Çekiş tipi seçin.")
            .IsInEnum().WithMessage("Çekiş tipi geçersiz.");
        RuleFor(x => x.PowerHp)
            .NotNull().WithMessage("Motor gücü zorunludur.")
            .InclusiveBetween(1, 2000).WithMessage("Motor gücü 1 ile 2000 hp arasında olmalıdır.");
        RuleFor(x => x.ExteriorColorId)
            .NotNull().WithMessage("Dış renk seçin.");
        RuleFor(x => x.InteriorColor)
            .MaximumLength(50).WithMessage("İç renk en fazla 50 karakter olabilir.");

        // Motor hacmi: elektrikli araçta boş olmalı, diğerlerinde zorunlu.
        When(x => x.FuelType == Enums.FuelType.Electric, () =>
        {
            RuleFor(x => x.EngineCc)
                .Null().WithMessage("Elektrikli araçta motor hacmi boş bırakılmalıdır.");
        }).Otherwise(() =>
        {
            RuleFor(x => x.EngineCc)
                .NotNull().WithMessage("Motor hacmi zorunludur.")
                .InclusiveBetween(50, 10000).WithMessage("Motor hacmi 50 ile 10.000 cc arasında olmalıdır.");
        });

        // Batarya ve menzil yalnızca elektrikli / plug-in hibrit araçta girilir.
        When(x => x.FuelType == Enums.FuelType.Electric || x.FuelType == Enums.FuelType.PlugInHybrid, () =>
        {
            RuleFor(x => x.BatteryKwh)
                .InclusiveBetween(1, 300).When(x => x.BatteryKwh.HasValue)
                .WithMessage("Batarya kapasitesi 1 ile 300 kWh arasında olmalıdır.");
            RuleFor(x => x.RangeKm)
                .InclusiveBetween(1, 2000).When(x => x.RangeKm.HasValue)
                .WithMessage("Menzil 1 ile 2000 km arasında olmalıdır.");
        }).Otherwise(() =>
        {
            RuleFor(x => x.BatteryKwh)
                .Null().WithMessage("Batarya kapasitesi yalnızca elektrikli ve plug-in hibrit araçta girilir.");
            RuleFor(x => x.RangeKm)
                .Null().WithMessage("Menzil yalnızca elektrikli ve plug-in hibrit araçta girilir.");
        });
    }
}
