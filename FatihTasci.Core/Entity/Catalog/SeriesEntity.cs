using FatihTasci.Core.Entity.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Catalog;

/// <summary>
/// Seri (örn. 3 Serisi). Slug aynı marka altında benzersizdir.
/// </summary>
[Table(name: "Series", Schema = "Catalog")]
public class SeriesEntity : BaseAuditableEntity<int>
{
    public int BrandId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public int SortOrder { get; set; }

    public virtual BrandEntity Brand { get; set; }
    public virtual ICollection<CarModelEntity> Models { get; set; } = new List<CarModelEntity>();
}
