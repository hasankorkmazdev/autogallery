using FatihTasci.Core.Models.Domain.Common;

namespace FatihTasci.Core.Models.Domain.Delivery;

public class DeliveryModel
{
    public int Id { get; set; }
    public string VehicleTitle { get; set; }
    public string CustomerName { get; set; }
    public string City { get; set; }
    public DateTime DeliveredAt { get; set; }
    public bool IsActive { get; set; }

    /// <summary>
    /// 800 px WebP (kart)
    /// </summary>
    public string MediumUrl { get; set; }

    /// <summary>
    /// 1600 px WebP (tam ekran)
    /// </summary>
    public string LargeUrl { get; set; }

    /// <summary>
    /// İsteğe bağlı video; varsa tam ekranda fotoğrafın yerine oynar. Yoksa null.
    /// </summary>
    public VideoFileModel Video { get; set; }
}
