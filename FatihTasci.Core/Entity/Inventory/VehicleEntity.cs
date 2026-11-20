using FatihTasci.Core.Entity.Base;
using FatihTasci.Core.Entity.Catalog;
using FatihTasci.Core.Entity.Identity;
using FatihTasci.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Inventory;

/// <summary>
/// Araç ilanı. Sihirbaz adım adım doldurduğu için taslakta teknik alanlar boş olabilir;
/// eksikler yayın kontrol listesinde gösterilir.
/// </summary>
[Table(name: "Vehicles", Schema = "Inventory")]
public class VehicleEntity : BaseAuditableEntity<Guid>
{
    /// <summary>
    /// Sitede "İlan No" olarak görünür. 100000'den başlayan sequence ile veritabanı verir.
    /// </summary>
    public int ListingNo { get; set; }

    /// <summary>
    /// Yıl + marka + model'den otomatik üretilir. Benzersiz değildir, adreste ilan no ile birlikte kullanılır.
    /// </summary>
    public string Slug { get; set; }

    public VehicleStatus Status { get; set; } = VehicleStatus.Draft;

    // --- Adım 1: Kimlik
    public int BrandId { get; set; }
    public int SeriesId { get; set; }
    public int CarModelId { get; set; }
    public string Title { get; set; }

    // --- Adım 2: Teknik
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

    // --- Adım 3: Durum
    public VehicleCondition Condition { get; set; } = VehicleCondition.Used;
    public bool HasWarranty { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public bool HeavyDamageRecord { get; set; }
    public PlateOrigin PlateOrigin { get; set; } = PlateOrigin.Turkish;
    public bool AcceptsTrade { get; set; }

    /// <summary>
    /// Boş = Tramer kaydı yok.
    /// </summary>
    public decimal? TramerAmount { get; set; }

    // --- Adım 6: Medya
    /// <summary>
    /// İsteğe bağlı YouTube bağlantısı (araç sayfasına gömülür). Yüklenen video aşağıdaki Video* alanlarındadır.
    /// </summary>
    public string VideoUrl { get; set; }
    public string ExpertiseReportPath { get; set; }

    /// <summary>
    /// Panelden yüklenen video (MP4 olduğu gibi, en çok 100 MB; sunucuda sıkıştırma ve filigran yok). Vitrinde
    /// galeride fotoğrafların yanında oynatılır. Afiş videonun ilk karesidir: 1280 px (oynatıcı) ve 480 px (küçük resim)
    /// WebP. VideoWidth / VideoHeight afişin boyutudur (oranı videoyla aynı). Yollar Storage kökünce görelidir (/media).
    /// </summary>
    public string VideoPath { get; set; }
    public string VideoPosterPath { get; set; }
    public string VideoPosterSmallPath { get; set; }
    public int? VideoWidth { get; set; }
    public int? VideoHeight { get; set; }

    // --- Adım 7: Fiyat ve yayın
    public decimal? Price { get; set; }

    /// <summary>
    /// Kampanyalı fiyat. Doluysa sitede Price üstü çizili gösterilir, bu fiyat öne çıkar; sıralama ve filtre bunu kullanır.
    /// Price'tan küçük olmalıdır.
    /// </summary>
    public decimal? CampaignPrice { get; set; }

    /// <summary>
    /// Kapalıysa sitede "Fiyat için arayınız" yazar. Fiyat yine de panelde tutulur.
    /// </summary>
    public bool ShowPrice { get; set; } = true;

    /// <summary>
    /// Sunucuda HtmlSanitizer ile temizlenmiş açıklama.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Aracın satış danışmanı (panel kullanıcısı). Yeni araçta aracı ekleyen kullanıcıdır, Fiyat adımında değişir.
    /// Sitede ilanın iletişim kutusunda adı görünür; "Ara" danışmanın telefonunu arar, cep telefonuysa WhatsApp da ona
    /// gider. Danışmanın telefonu yoksa ya da danışman pasifse galerinin numaraları kullanılır.
    /// </summary>
    public Guid? AdvisorId { get; set; }

    public bool IsFeatured { get; set; }
    public int FeaturedOrder { get; set; }

    /// <summary>
    /// Fırsat aracı: ana sayfadaki araç kaydırıcısının "Fırsat Araçları" sekmesinde gösterilir (panelde Fiyat adımında işaretlenir).
    /// </summary>
    public bool IsOpportunity { get; set; }

    public int ViewCount { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? SoldAt { get; set; }

    public virtual BrandEntity Brand { get; set; }
    public virtual SeriesEntity Series { get; set; }
    public virtual CarModelEntity CarModel { get; set; }
    public virtual ColorEntity ExteriorColor { get; set; }
    public virtual UserEntity Advisor { get; set; }

    public virtual ICollection<VehicleBodyPartEntity> BodyParts { get; set; } = new List<VehicleBodyPartEntity>();
    public virtual ICollection<VehicleFeatureEntity> Features { get; set; } = new List<VehicleFeatureEntity>();
    public virtual ICollection<VehicleMediaEntity> Media { get; set; } = new List<VehicleMediaEntity>();
}
