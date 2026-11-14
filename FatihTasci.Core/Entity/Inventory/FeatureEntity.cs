using FatihTasci.Core.Entity.Base;
using FatihTasci.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Inventory;

/// <summary>
/// Donanım listesi (panelden yönetilir).
/// </summary>
[Table(name: "Features", Schema = "Inventory")]
public class FeatureEntity : BaseEntity<int>
{
    public FeatureCategory Category { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; }
}
