using FatihTasci.Core.Models.Domain.Setting;

namespace FatihTasci.Core.Models.Domain.Public;

// Vitrin (Razor) sayfalarının modelleri. Değerler ekranda gösterilecek şekilde hazırlanır.

/// <summary>
/// Her vitrin sayfasının layout'ta kullandığı ortak bilgiler.
/// </summary>
public class SiteContextModel
{
    public SiteSettingsModel Settings { get; set; }
}
