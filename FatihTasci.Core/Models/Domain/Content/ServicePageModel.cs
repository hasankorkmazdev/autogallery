using FatihTasci.Core.Models.Domain.Lead;

namespace FatihTasci.Core.Models.Domain.Content;

/// <summary>
/// Vitrindeki hizmet sayfası: gösterilen hizmet ve sayfa altındaki diğer hizmetler.
/// </summary>
public class ServicePageModel
{
    public ServiceContentModel Service { get; set; }
    public List<ServiceContentModel> Others { get; set; } = new();

    /// <summary>
    /// Yalnız "Aracımı Satmak İstiyorum" sayfasında dolu: fotoğraflı teklif formu ve marka önerileri (katalogdaki aktif markalar).
    /// </summary>
    public CreateSellLeadRequestModel SellForm { get; set; }
    public List<string> SellBrands { get; set; } = new();
}
