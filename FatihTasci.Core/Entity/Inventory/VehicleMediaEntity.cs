using FatihTasci.Core.Entity.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Inventory;

/// <summary>
/// Araç fotoğrafı. Orijinal dosya saklanır, ayrıca 480 / 1280 / 2400 px WebP varyantları üretilir.
/// Yollar Storage kökünce görelidir ve sitede /media altında sunulur.
/// </summary>
[Table(name: "VehicleMedia", Schema = "Inventory")]
public class VehicleMediaEntity : BaseEntity<Guid>
{
    public Guid VehicleId { get; set; }
    public string OriginalPath { get; set; }
    public string SmallPath { get; set; }
    public string MediumPath { get; set; }
    public string LargePath { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int SortOrder { get; set; }
    public bool IsCover { get; set; }
    public string AltText { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;

    public virtual VehicleEntity Vehicle { get; set; }
}
