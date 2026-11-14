using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Inventory;

[Table(name: "VehicleFeatures", Schema = "Inventory")]
public class VehicleFeatureEntity
{
    public Guid VehicleId { get; set; }
    public int FeatureId { get; set; }

    public virtual VehicleEntity Vehicle { get; set; }
    public virtual FeatureEntity Feature { get; set; }
}
