namespace FatihTasci.Core.Models.Domain.Catalog;

public class SeriesModel
{
    public int Id { get; set; }
    public int BrandId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int ModelCount { get; set; }
}
