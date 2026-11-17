namespace FatihTasci.Core.Models.Domain.Public;

// Vitrin (Razor) sayfalarının modelleri. Değerler ekranda gösterilecek şekilde hazırlanır.

public class ListingsPageModel
{
    public ListingFilterModel Filter { get; set; }
    public List<VehicleCardModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public string BrandName { get; set; }
    public string SeriesName { get; set; }
    public List<FilterOptionModel> Brands { get; set; } = new();
    public List<FilterOptionModel> Series { get; set; } = new();
}
