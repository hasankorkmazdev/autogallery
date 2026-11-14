namespace FatihTasci.Core.Models.Domain.Public;

// Vitrin (Razor) sayfalarının modelleri. Değerler ekranda gösterilecek şekilde hazırlanır.

public class GalleryImageModel
{
    public string SmallUrl { get; set; }
    public string MediumUrl { get; set; }
    public string LargeUrl { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Alt { get; set; }
}
