using FatihTasci.Core.Enums;

namespace FatihTasci.Core.Models.Domain.Public;

/// <summary>
/// Aracın paylaşım görseli için gereken bilgiler (görüntülenme sayısını artırmadan okunur).
/// </summary>
public class VehicleShareModel
{
    public int ListingNo { get; set; }
    public string Title { get; set; }
    public List<string> Summary { get; set; } = new();
    public decimal? Price { get; set; }
    public decimal? CampaignPrice { get; set; }
    public bool ShowPrice { get; set; }
    public VehicleStatus Status { get; set; }

    /// <summary>
    /// Kapak fotoğrafının büyük varyantı (/media/...). Fotoğraf yoksa null.
    /// </summary>
    public string CoverUrl { get; set; }
}
