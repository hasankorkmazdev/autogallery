using FatihTasci.Core.Enums;

namespace FatihTasci.Core.Models.Domain.Public;

// Vitrin (Razor) sayfalarının modelleri. Değerler ekranda gösterilecek şekilde hazırlanır.

public class VehicleCardModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    /// <summary>
    /// Kapak fotoğrafı: 1280 (CoverUrl), 480 ve 2400 varyantları (srcset)
    /// </summary>
    public string CoverUrl { get; set; }
    public string CoverSmallUrl { get; set; }
    public string CoverLargeUrl { get; set; }
    public string CoverAlt { get; set; }

    /// <summary>
    /// Kart başlığı: marka (kalın) + model (Series), altında paket (CarModel). Title görsel alt metnidir.
    /// </summary>
    public string BrandName { get; set; }
    public string SeriesName { get; set; }
    public string ModelName { get; set; }
    public int? Year { get; set; }
    public int? Kilometer { get; set; }
    public string FuelName { get; set; }
    public string TransmissionName { get; set; }
    public decimal? Price { get; set; }

    /// <summary>
    /// Doluysa Price üstü çizili gösterilir.
    /// </summary>
    public decimal? CampaignPrice { get; set; }
    public bool ShowPrice { get; set; }
    public VehicleStatus Status { get; set; }
}
