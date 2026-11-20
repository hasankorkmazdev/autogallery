using FatihTasci.Core.Enums;

namespace FatihTasci.Core.Models.Domain.Public;

// Vitrin (Razor) sayfalarının modelleri. Değerler ekranda gösterilecek şekilde hazırlanır.

public class PublicVehicleDetailModel
{
    public Guid Id { get; set; }
    public int ListingNo { get; set; }
    public string Slug { get; set; }
    public string Url { get; set; }
    public string Title { get; set; }
    public string BrandName { get; set; }
    public string BrandSlug { get; set; }
    public string BrandLogoUrl { get; set; }
    public string SeriesName { get; set; }
    public string SeriesSlug { get; set; }
    public string ModelName { get; set; }
    public VehicleStatus Status { get; set; }
    public decimal? Price { get; set; }

    /// <summary>
    /// Doluysa Price üstü çizili gösterilir.
    /// </summary>
    public decimal? CampaignPrice { get; set; }
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

    /// <summary>
    /// Satış danışmanı ve iletişim telefonu (yalnız danışman aktifse dolu). Telefon boşsa galeri telefonu kullanılır.
    /// </summary>
    public string AdvisorName { get; set; }
    public string AdvisorPhone { get; set; }

    public DateTime? PublishedAt { get; set; }
    public DateTime? SoldAt { get; set; }
    public string VideoEmbedUrl { get; set; }
    public string VideoUrl { get; set; }
    public List<GalleryImageModel> Images { get; set; } = new();

    /// <summary>
    /// Paylaşım görseli (og:image) bilgileri
    /// </summary>
    public VehicleShareModel Share { get; set; }

    // schema.org Car için ham değerler (Specs ekranda gösterilecek metinlerdir)
    public int? Year { get; set; }
    public int? Kilometer { get; set; }
    public string FuelName { get; set; }
    public string TransmissionName { get; set; }
    public string BodyTypeName { get; set; }
    public int? EngineCc { get; set; }
    public int? PowerHp { get; set; }
    public Drivetrain? Drivetrain { get; set; }
    public string ExteriorColorName { get; set; }
    public string InteriorColor { get; set; }
    public VehicleCondition Condition { get; set; }
    public List<VehicleCardModel> Similar { get; set; } = new();
}
