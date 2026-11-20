using FatihTasci.Core.Models.Domain.Delivery;
using FatihTasci.Core.Models.Domain.Showroom;

namespace FatihTasci.Core.Models.Domain.Public;

// Vitrin (Razor) sayfalarının modelleri. Değerler ekranda gösterilecek şekilde hazırlanır.

public class HomePageModel
{
    /// <summary>
    /// Araç kaydırıcısının "Vitrin" sekmesi (en çok PublicVehicleService.ShowcaseSize): panelde "Vitrinde öne çıkar"
    /// işaretliler vitrin sırasıyla, eksik kalırsa en yeni araçlarla tamamlanır.
    /// </summary>
    public List<VehicleCardModel> Showcase { get; set; } = new();

    /// <summary>
    /// "Yeni Gelenler" sekmesi: Vitrin sekmesinde olmayan en yeni araçlar
    /// </summary>
    public List<VehicleCardModel> Latest { get; set; } = new();

    /// <summary>
    /// "Fırsat Araçları" sekmesi: panelde "Fırsat aracı" işaretli yayındaki araçlar (en yeni önce, en çok 30).
    /// </summary>
    public List<VehicleCardModel> Opportunities { get; set; } = new();
    public int ListingCount { get; set; }

    /// <summary>
    /// İlanda aracı olan markalar (logolarıyla)
    /// </summary>
    public List<FilterOptionModel> Brands { get; set; } = new();

    /// <summary>
    /// Girişteki hızlı aramanın Model seçenekleri: ilanda aracı olan seriler (Parent = marka slug'ı)
    /// </summary>
    public List<FilterOptionModel> Series { get; set; } = new();

    /// <summary>
    /// Galeri atmosferi sahneleri (aktif ve fotoğraflı). Boşsa bölüm gösterilmez.
    /// </summary>
    public List<ShowroomSceneModel> Scenes { get; set; } = new();

    /// <summary>
    /// Yayındaki en yeni teslimatlar. Boşsa bölüm gösterilmez.
    /// </summary>
    public List<DeliveryModel> Deliveries { get; set; } = new();
}
