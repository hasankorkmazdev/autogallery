namespace FatihTasci.Core.Models.Domain.Catalog;

public class BrandModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }

    /// <summary>
    /// Logo adresi, örn. /images/brands/bmw.png. Logosu yoksa null.
    /// </summary>
    public string LogoUrl { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int SeriesCount { get; set; }
}
