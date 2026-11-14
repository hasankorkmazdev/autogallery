using FatihTasci.Core.Entity.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Catalog;

/// <summary>
/// Model (motor + paket, örn. 320i M Sport). Slug aynı seri altında benzersizdir.
/// </summary>
[Table(name: "CarModels", Schema = "Catalog")]
public class CarModelEntity : BaseAuditableEntity<int>
{
    public int SeriesId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }

    public virtual SeriesEntity Series { get; set; }
}
