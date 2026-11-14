using FatihTasci.Core.Enums;

namespace FatihTasci.Core.Models.Domain.Public;

// Vitrin (Razor) sayfalarının modelleri. Değerler ekranda gösterilecek şekilde hazırlanır.

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

    /// <summary>
    /// Doluysa Price üstü çizili gösterilir.
    /// </summary>
    public decimal? CampaignPrice { get; set; }
    public bool ShowPrice { get; set; }
    public VehicleStatus Status { get; set; }
}
