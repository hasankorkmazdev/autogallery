using FatihTasci.Core.Models.Domain.Delivery;
using FatihTasci.Core.Models.Domain.Showroom;

namespace FatihTasci.Core.Models.Domain.Public;

// Vitrin (Razor) sayfalarının modelleri. Değerler ekranda gösterilecek şekilde hazırlanır.

public class HomePageModel
{
    /// <summary>
    /// İlk ekrandaki mozaik (en çok 5 araç): panelde "Vitrinde öne çıkar" işaretliler vitrin sırasıyla,
    /// 5'ten azsa en yeni araçlarla tamamlanır.
    /// </summary>
    public List<VehicleCardModel> Showcase { get; set; } = new();

    /// <summary>
    /// Mozaikte olmayan en yeni araçlar
    /// </summary>
    public List<VehicleCardModel> Latest { get; set; } = new();

    /// <summary>
    /// Panelde "Fırsat aracı" işaretli yayındaki araçlar (en yeni önce, en çok 30). Mozaiğin altındaki akan yazı şeridinde gösterilir.
    /// </summary>
    public List<VehicleCardModel> Opportunities { get; set; } = new();
    public int ListingCount { get; set; }

    /// <summary>
    /// İlanda aracı olan markalar (logolarıyla)
    /// </summary>
    public List<FilterOptionModel> Brands { get; set; } = new();

    /// <summary>
    /// Galeri atmosferi sahneleri (aktif ve fotoğraflı). Boşsa bölüm gösterilmez.
    /// </summary>
    public List<ShowroomSceneModel> Scenes { get; set; } = new();

    /// <summary>
    /// Yayındaki en yeni teslimatlar. Boşsa bölüm gösterilmez.
    /// </summary>
    public List<DeliveryModel> Deliveries { get; set; } = new();
}
