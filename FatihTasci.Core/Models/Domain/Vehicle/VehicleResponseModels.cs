using FatihTasci.Core.Enums;

namespace FatihTasci.Core.Models.Domain.Vehicle;

public class VehicleListItemModel
{
    public Guid Id { get; set; }
    public int ListingNo { get; set; }
    public string Title { get; set; }
    public string BrandName { get; set; }
    public string SeriesName { get; set; }
    public string ModelName { get; set; }
    public int? Year { get; set; }
    public int? Kilometer { get; set; }
    public decimal? Price { get; set; }
    public bool ShowPrice { get; set; }
    public VehicleStatus Status { get; set; }
    public string CoverUrl { get; set; }
    public int PhotoCount { get; set; }
    public bool IsFeatured { get; set; }
    public string PublicUrl { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? LastUpdateDate { get; set; }
}

public class VehicleMediaModel
{
    public Guid Id { get; set; }
    public string SmallUrl { get; set; }
    public string MediumUrl { get; set; }
    public string LargeUrl { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int SortOrder { get; set; }
    public bool IsCover { get; set; }
}

/// <summary>
/// Sihirbazın kullandığı tam kayıt.
/// </summary>
public class VehicleDetailModel
{
    public Guid Id { get; set; }
    public int ListingNo { get; set; }
    public string Slug { get; set; }
    public VehicleStatus Status { get; set; }
    public string PublicUrl { get; set; }

    public int BrandId { get; set; }
    public int SeriesId { get; set; }
    public int CarModelId { get; set; }
    public string BrandName { get; set; }
    public string SeriesName { get; set; }
    public string ModelName { get; set; }
    public string Title { get; set; }

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

    public VehicleCondition Condition { get; set; }
    public bool HasWarranty { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public bool HeavyDamageRecord { get; set; }
    public PlateOrigin PlateOrigin { get; set; }
    public bool AcceptsTrade { get; set; }
    public decimal? TramerAmount { get; set; }

    public List<VehicleBodyPartItemModel> BodyParts { get; set; } = new();
    public List<int> FeatureIds { get; set; } = new();
    public List<VehicleMediaModel> Media { get; set; } = new();
    public string VideoUrl { get; set; }
    public string ExpertiseReportUrl { get; set; }

    public decimal? Price { get; set; }
    public bool ShowPrice { get; set; }
    public string Description { get; set; }
    public bool IsFeatured { get; set; }
    public int FeaturedOrder { get; set; }

    public int ViewCount { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? SoldAt { get; set; }
}

public class PublishCheckItemModel
{
    public string Key { get; set; }
    public string Label { get; set; }
    public bool Passed { get; set; }

    /// <summary>
    /// true ise eksik olduğu sürece araç yayınlanamaz; false ise yalnızca uyarıdır.
    /// </summary>
    public bool Blocking { get; set; }

    /// <summary>
    /// Eksiği gidermek için gidilecek sihirbaz adımı (1-7)
    /// </summary>
    public int Step { get; set; }
}

public class PublishCheckModel
{
    public bool CanPublish { get; set; }
    public List<PublishCheckItemModel> Items { get; set; } = new();
}
