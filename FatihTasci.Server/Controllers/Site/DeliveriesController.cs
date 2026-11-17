using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Site;

/// <summary>
/// Teslimatlar sayfası (/teslimatlar): yayındaki teslimatlar, en yeni önce, sayfalı. Panelde Teslimatlar'dan yönetilir.
/// </summary>
public class DeliveriesController : Controller
{
    private readonly IDeliveryService _deliveryService;

    public DeliveriesController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    [HttpGet("teslimatlar")]
    public async Task<IActionResult> Index(int page = 1)
    {
        return View(await _deliveryService.GetPublicPageAsync(page));
    }
}
