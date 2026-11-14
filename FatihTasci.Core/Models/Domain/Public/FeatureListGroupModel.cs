namespace FatihTasci.Core.Models.Domain.Public;

// Vitrin (Razor) sayfalarının modelleri. Değerler ekranda gösterilecek şekilde hazırlanır.

public class FeatureListGroupModel
{
    public string CategoryName { get; set; }
    public List<string> Features { get; set; } = new();
}
