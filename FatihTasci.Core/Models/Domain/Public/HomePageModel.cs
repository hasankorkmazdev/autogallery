using FatihTasci.Core.Models.Domain.Showroom;

namespace FatihTasci.Core.Models.Domain.Public;

// Vitrin (Razor) sayfalarının modelleri. Değerler ekranda gösterilecek şekilde hazırlanır.

public class HomePageModel
{
    public List<VehicleCardModel> Featured { get; set; } = new();
    public List<VehicleCardModel> Latest { get; set; } = new();
    public int StockCount { get; set; }

    /// <summary>
    /// Stokta aracı olan markalar (logolarıyla)
    /// </summary>
    public List<FilterOptionModel> Brands { get; set; } = new();

    /// <summary>
    /// Galeri atmosferi sahneleri (aktif ve fotoğraflı). Boşsa bölüm gösterilmez.
    /// </summary>
    public List<ShowroomSceneModel> Scenes { get; set; } = new();
}
