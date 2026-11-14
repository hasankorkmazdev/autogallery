using FatihTasci.Core.Entity.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Common;

/// <summary>
/// Site ayarları (anahtar-değer). Anahtarlar SiteSettingKeys'te tanımlıdır.
/// </summary>
[Table(name: "SiteSettings", Schema = "Common")]
public class SiteSettingEntity : BaseEntity<int>
{
    public string Key { get; set; }
    public string Value { get; set; }
}
