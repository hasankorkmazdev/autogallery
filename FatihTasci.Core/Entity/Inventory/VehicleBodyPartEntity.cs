using FatihTasci.Core.Entity.Base;
using FatihTasci.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Inventory;

/// <summary>
/// Boya / değişen şemasında bir parçanın durumu. (VehicleId, Part) benzersizdir.
/// </summary>
[Table(name: "VehicleBodyParts", Schema = "Inventory")]
public class VehicleBodyPartEntity : BaseEntity<int>
{
    public Guid VehicleId { get; set; }
    public BodyPart Part { get; set; }
    public BodyPartState State { get; set; } = BodyPartState.Original;
    public string Note { get; set; }

    public virtual VehicleEntity Vehicle { get; set; }
}
