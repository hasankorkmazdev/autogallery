using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Site;

// Vitrin sayfaları (Razor, sunucuda render). Faz 1: sade tasarım.

public class HomeController : Controller
{
    private readonly IPublicVehicleService _publicVehicleService;
    // Ana sayfadaki teslimat slaytında dönen teslimat sayısı
    private const int HomeDeliveryCount = 8;

    private readonly IShowroomService _showroomService;
    private readonly IDeliveryService _deliveryService;

    public HomeController(IPublicVehicleService publicVehicleService, IShowroomService showroomService, IDeliveryService deliveryService)
    {
        _publicVehicleService = publicVehicleService;
        _showroomService = showroomService;
        _deliveryService = deliveryService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var model = await _publicVehicleService.GetHomeAsync();
        model.Scenes = await _showroomService.GetPublicAsync();
        model.Deliveries = await _deliveryService.GetLatestPublicAsync(HomeDeliveryCount);
        return View(model);
    }

    [HttpGet("kvkk")]
    public IActionResult Kvkk()
    {
        return View();
    }

    [Route("hata/{code:int?}")]
    public IActionResult Error(int? code)
    {
        var statusCode = code ?? 500;
        var feature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        Response.StatusCode = feature == null && code == null ? 500 : statusCode;
        return View(statusCode);
    }
}
