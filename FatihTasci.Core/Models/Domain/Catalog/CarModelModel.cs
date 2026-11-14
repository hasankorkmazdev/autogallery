namespace FatihTasci.Core.Models.Domain.Catalog;

public class CarModelModel
{
    public int Id { get; set; }
    public int SeriesId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public bool IsActive { get; set; }
}
