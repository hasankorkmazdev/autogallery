using FatihTasci.Core.Entity.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Crm;

/// <summary>
/// "Aracını Sat" formundan yüklenen fotoğraflar. Dosya özel klasördedir (private/leads/...),
/// sitede sunulmaz; panel yalnız yetkili uçtan (GET /api/leads/{id}/media/{mediaId}) alır.
/// </summary>
[Table(name: "LeadMedia", Schema = "Crm")]
public class LeadMediaEntity : BaseEntity<Guid>
{
    public Guid LeadId { get; set; }

    /// <summary>
    /// Özel köke göre göreli yol (EXIF temizlenmiş, küçültülmüş WebP).
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Ziyaretçinin seçtiği sıra
    /// </summary>
    public int SortOrder { get; set; }

    public virtual LeadEntity Lead { get; set; }
}
