namespace FatihTasci.Core.Models.Domain.Public;

// Vitrin (Razor) sayfalarının modelleri. Değerler ekranda gösterilecek şekilde hazırlanır.

public class FilterOptionModel
{
    public string Value { get; set; }
    public string Label { get; set; }
    public int Count { get; set; }

    /// <summary>
    /// Yalnız marka seçeneklerinde: logo adresi
    /// </summary>
    public string LogoUrl { get; set; }

    /// <summary>
    /// Yalnız ana sayfa aramasındaki seri (Model) seçeneklerinde: markanın slug'ı
    /// </summary>
    public string Parent { get; set; }
}
