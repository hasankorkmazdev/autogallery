using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Vehicle;

// Kurallar panelin zod şemalarıyla (FatihTasci.Admin/src/pages/vehicles/wizard/schemas.ts) aynıdır.
// Adımlar arası kurallar (sıfır araçta km, yayın için fiyat) VehicleService'te kontrol edilir.

public class VehicleListRequestModel : PagedListRequestModel
{
    public VehicleStatus? Status { get; set; }
    public int? BrandId { get; set; }
    public string Search { get; set; }
}
public class VehicleListRequestModelValidator : AbstractValidator<VehicleListRequestModel>
{
    public VehicleListRequestModelValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0).WithMessage("Sayfa numarası geçersiz.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır.");
        RuleFor(x => x.Search).MaximumLength(100).WithMessage("Arama en fazla 100 karakter olabilir.");
    }
}

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
        RuleFor(x => x.SeriesId).GreaterThan(0).WithMessage("Seri seçin.");
        RuleFor(x => x.CarModelId).GreaterThan(0).WithMessage("Model seçin.");
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("İlan başlığı zorunludur.")
            .MaximumLength(150).WithMessage("İlan başlığı en fazla 150 karakter olabilir.");
    }
}

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

/// <summary>
/// Adım 4: Boya / değişen. Gönderilmeyen parça Orijinal kabul edilir.
/// </summary>
public class VehicleBodyPartsRequestModel : IRequestModel
{
    public List<VehicleBodyPartItemModel> Parts { get; set; } = new();
}
public class VehicleBodyPartItemModel
{
    public BodyPart Part { get; set; }
    public BodyPartState State { get; set; }
    public string Note { get; set; }
}
public class VehicleBodyPartsRequestModelValidator : AbstractValidator<VehicleBodyPartsRequestModel>
{
    public VehicleBodyPartsRequestModelValidator()
    {
        RuleFor(x => x.Parts)
            .NotNull().WithMessage("Parça listesi gönderilmelidir.")
            .Must(x => x.Select(p => p.Part).Distinct().Count() == x.Count)
            .WithMessage("Aynı parça birden fazla kez gönderilemez.");
        RuleForEach(x => x.Parts).ChildRules(part =>
        {
            part.RuleFor(p => p.Part).IsInEnum().WithMessage("Parça geçersiz.");
            part.RuleFor(p => p.State).IsInEnum().WithMessage("Parça durumu geçersiz.");
            part.RuleFor(p => p.Note).MaximumLength(200).WithMessage("Not en fazla 200 karakter olabilir.");
        });
    }
}

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

/// <summary>
/// Adım 6: Video bağlantısı
/// </summary>
public class VehicleVideoRequestModel : IRequestModel
{
    public string VideoUrl { get; set; }
}
public class VehicleVideoRequestModelValidator : AbstractValidator<VehicleVideoRequestModel>
{
    public VehicleVideoRequestModelValidator()
    {
        RuleFor(x => x.VideoUrl)
            .MaximumLength(300).WithMessage("Video bağlantısı en fazla 300 karakter olabilir.")
            .Must(BeHttpsUrl).When(x => !string.IsNullOrWhiteSpace(x.VideoUrl))
            .WithMessage("Geçerli bir https bağlantısı girin.");
    }

    private static bool BeHttpsUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
    }
}

/// <summary>
/// Adım 7: Fiyat, açıklama ve vitrin
/// </summary>
public class VehiclePricingRequestModel : IRequestModel
{
    public decimal? Price { get; set; }
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
        RuleFor(x => x.Description)
            .MaximumLength(10000).WithMessage("Açıklama en fazla 10.000 karakter olabilir.");
        RuleFor(x => x.FeaturedOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Vitrin sırası 0 ya da daha büyük olmalıdır.");
    }
}

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
