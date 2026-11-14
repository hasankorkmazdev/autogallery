using FatihTasci.Core.Entity.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Common;

/// <summary>
/// Ana sayfadaki "Galeri Atmosferi" bölümünün sahneleri (showroom fotoğrafları).
/// Orijinal fotoğraf özel klasörde, 1280 / 2400 px WebP varyantları /media altında saklanır.
/// Vitrinde yalnızca aktif ve fotoğrafı olan sahneler gösterilir.
/// </summary>
[Table(name: "ShowroomScenes", Schema = "Common")]
public class ShowroomSceneEntity : BaseAuditableEntity<int>
{
    /// <summary>
    /// Kısa etiket (örn. "Sergi salonu"); dar panelde dikey yazılır.
    /// </summary>
    public string Tag { get; set; }

    public string Title { get; set; }
    public int SortOrder { get; set; }

    public string OriginalPath { get; set; }
    public string MediumPath { get; set; }
    public string LargePath { get; set; }
}
