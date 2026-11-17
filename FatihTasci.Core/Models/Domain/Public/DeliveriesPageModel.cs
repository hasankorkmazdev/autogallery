using FatihTasci.Core.Models.Domain.Delivery;

namespace FatihTasci.Core.Models.Domain.Public;

/// <summary>
/// Vitrin /teslimatlar: yayındaki teslimatlar, en yeni önce, sayfalı.
/// </summary>
public class DeliveriesPageModel
{
    public List<DeliveryModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
}
