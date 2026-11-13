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

/// <summary>
/// Dış renk listesi (lookup).
/// </summary>
[Table(name: "Colors", Schema = "Catalog")]
public class ColorEntity : BaseEntity<int>
{
    public string Name { get; set; }
    public string HexCode { get; set; }
    public int SortOrder { get; set; }
}
