using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Site;

// Vitrin sayfaları (Razor, sunucuda render). Faz 1: sade tasarım.

public class HomeController : Controller
{
    private readonly IPublicVehicleService _publicVehicleService;
    private readonly IShowroomService _showroomService;

    public HomeController(IPublicVehicleService publicVehicleService, IShowroomService showroomService)
    {
        _publicVehicleService = publicVehicleService;
        _showroomService = showroomService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var model = await _publicVehicleService.GetHomeAsync();
        model.Scenes = await _showroomService.GetPublicAsync();
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
