using FatihTasci.Core.Entity.Base;
using FatihTasci.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Common;

/// <summary>
/// Marka / seri / model adı değişince eski slug burada saklanır, eski adres 301 ile yenisine yönlendirilir.
/// ParentId: seri için BrandId, model için SeriesId, marka için null.
/// </summary>
[Table(name: "SlugRedirects", Schema = "Common")]
public class SlugRedirectEntity : BaseEntity<int>
{
    public SlugEntityType EntityType { get; set; }
    public int? ParentId { get; set; }
    public string OldSlug { get; set; }
    public int EntityId { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}
