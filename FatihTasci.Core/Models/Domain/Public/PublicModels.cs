using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Setting;

namespace FatihTasci.Core.Models.Domain.Public;

// Vitrin (Razor) sayfalarının modelleri. Değerler ekranda gösterilecek şekilde hazırlanır.

/// <summary>
/// /stok filtreleri. Marka ve seri adresten (slug), diğerleri query string'den gelir.
/// </summary>
public class StockFilterModel
{
    public string Brand { get; set; }
    public string Series { get; set; }
    public FuelType? Fuel { get; set; }
    public TransmissionType? Transmission { get; set; }
    public BodyType? Body { get; set; }
    public int? YearMin { get; set; }
    public int? YearMax { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public int? KmMax { get; set; }

    /// <summary>
    /// yeni (varsayılan), fiyat-artan, fiyat-azalan, km, yil
    /// </summary>
    public string Sort { get; set; }

    public int Page { get; set; } = 1;
}

public class VehicleCardModel
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string CoverUrl { get; set; }
    public string CoverAlt { get; set; }
    public int? Year { get; set; }
    public int? Kilometer { get; set; }
    public string FuelName { get; set; }
    public string TransmissionName { get; set; }
    public decimal? Price { get; set; }
    public bool ShowPrice { get; set; }
    public VehicleStatus Status { get; set; }
}

public class FilterOptionModel
{
    public string Value { get; set; }
    public string Label { get; set; }
    public int Count { get; set; }
}

public class StockPageModel
{
    public StockFilterModel Filter { get; set; }
    public List<VehicleCardModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public string BrandName { get; set; }
    public string SeriesName { get; set; }
    public List<FilterOptionModel> Brands { get; set; } = new();
    public List<FilterOptionModel> Series { get; set; } = new();
}

public class SpecItemModel
{
    public string Label { get; set; }
    public string Value { get; set; }
}

public class FeatureListGroupModel
{
    public string CategoryName { get; set; }
    public List<string> Features { get; set; } = new();
}

public class BodyPartViewModel
{
    public BodyPart Part { get; set; }
    public string PartName { get; set; }
    public BodyPartState State { get; set; }
    public string StateName { get; set; }
    public string Note { get; set; }
}

public class GalleryImageModel
{
    public string SmallUrl { get; set; }
    public string MediumUrl { get; set; }
    public string LargeUrl { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Alt { get; set; }
}

public class PublicVehicleDetailModel
{
    public Guid Id { get; set; }
    public int ListingNo { get; set; }
    public string Slug { get; set; }
    public string Url { get; set; }
    public string Title { get; set; }
    public string BrandName { get; set; }
    public string BrandSlug { get; set; }
    public string SeriesName { get; set; }
    public string SeriesSlug { get; set; }
    public string ModelName { get; set; }
    public VehicleStatus Status { get; set; }
    public decimal? Price { get; set; }
    public bool ShowPrice { get; set; }

    /// <summary>
    /// Özet çubuğu: yıl · km · yakıt · vites · kasa · güç
    /// </summary>
    public List<string> Summary { get; set; } = new();

    public List<SpecItemModel> Specs { get; set; } = new();
    public List<FeatureListGroupModel> Features { get; set; } = new();
    public List<BodyPartViewModel> BodyParts { get; set; } = new();
    public bool HeavyDamageRecord { get; set; }
    public decimal? TramerAmount { get; set; }
    public string ExpertiseReportUrl { get; set; }
    public string Description { get; set; }
    public string VideoEmbedUrl { get; set; }
    public string VideoUrl { get; set; }
    public List<GalleryImageModel> Images { get; set; } = new();
    public List<VehicleCardModel> Similar { get; set; } = new();
}

public class HomePageModel
{
    public List<VehicleCardModel> Featured { get; set; } = new();
    public List<VehicleCardModel> Latest { get; set; } = new();
    public int StockCount { get; set; }
}

/// <summary>
/// Her vitrin sayfasının layout'ta kullandığı ortak bilgiler.
/// </summary>
public class SiteContextModel
{
    public SiteSettingsModel Settings { get; set; }
}
