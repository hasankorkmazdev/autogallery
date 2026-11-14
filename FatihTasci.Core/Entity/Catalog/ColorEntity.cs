using FatihTasci.Core.Entity.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Catalog;

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
