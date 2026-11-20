using FatihTasci.Core.Entity.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Crm;

/// <summary>
/// Müşteriye teslim edilen aracın fotoğrafı (vitrinde ana sayfa ve /teslimatlar). Her teslimatın tek fotoğrafı ve
/// isteğe bağlı tek videosu olur.
/// Orijinal özel klasörde, EXIF'siz 800 / 1600 px WebP varyantları /media/deliveries altında saklanır.
/// IsActive kapalıysa vitrinde görünmez.
/// </summary>
[Table(name: "Deliveries", Schema = "Crm")]
public class DeliveryEntity : BaseAuditableEntity<int>
{
    /// <summary>
    /// Ekranda yazılan araç adı (örn. "2021 BMW 320i M Sport"); ilana bağlı değildir.
    /// </summary>
    public string VehicleTitle { get; set; }

    /// <summary>
    /// İsteğe bağlı, kısaltılmış müşteri adı (örn. "Ahmet B.").
    /// </summary>
    public string CustomerName { get; set; }

    /// <summary>
    /// İsteğe bağlı şehir (örn. "İstanbul"); serbest metin, yurt dışı da yazılabilir.
    /// </summary>
    public string City { get; set; }

    public DateTime DeliveredAt { get; set; }

    public string OriginalPath { get; set; }
    public string MediumPath { get; set; }
    public string LargePath { get; set; }

    /// <summary>
    /// İsteğe bağlı teslimat videosu (MP4 olduğu gibi, en çok 100 MB; sunucuda sıkıştırma yok). Kartta fotoğraf
    /// görünür, tam ekranda video oynar. Afiş videonun ilk karesidir (1600 px WebP); VideoWidth / VideoHeight afişin
    /// boyutudur (oranı videoyla aynı). Yollar /media/deliveries altındadır.
    /// </summary>
    public string VideoPath { get; set; }
    public string VideoPosterPath { get; set; }
    public int? VideoWidth { get; set; }
    public int? VideoHeight { get; set; }
}
