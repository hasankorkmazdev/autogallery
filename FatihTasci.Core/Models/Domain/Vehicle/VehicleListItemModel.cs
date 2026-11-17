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
    public decimal? CampaignPrice { get; set; }
    public bool ShowPrice { get; set; }
    public VehicleStatus Status { get; set; }
    public string CoverUrl { get; set; }
    public int PhotoCount { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsOpportunity { get; set; }
    public string PublicUrl { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? LastUpdateDate { get; set; }
}
