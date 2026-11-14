using FatihTasci.Core.Enums;

namespace FatihTasci.Core.Models.Domain.Vehicle;

// Kurallar panelin zod şemalarıyla (FatihTasci.Admin/src/pages/vehicles/wizard/schemas.ts) aynıdır.
// Adımlar arası kurallar (sıfır araçta km, yayın için fiyat) VehicleService'te kontrol edilir.

public class VehicleBodyPartItemModel
{
    public BodyPart Part { get; set; }
    public BodyPartState State { get; set; }
    public string Note { get; set; }
}
