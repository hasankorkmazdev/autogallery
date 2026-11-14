using FatihTasci.Core.Entity.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Catalog;

/// <summary>
/// Marka (örn. BMW). Slug addan otomatik üretilir, tüm markalar arasında benzersizdir.
/// </summary>
[Table(name: "Brands", Schema = "Catalog")]
public class BrandEntity : BaseAuditableEntity<int>
{
    public string Name { get; set; }
    public string Slug { get; set; }
    public string LogoPath { get; set; }
    public int SortOrder { get; set; }

    public virtual ICollection<SeriesEntity> Series { get; set; } = new List<SeriesEntity>();
}
