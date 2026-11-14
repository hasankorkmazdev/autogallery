using FatihTasci.Core.Helpers;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Site;

// Vitrin sayfaları (Razor, sunucuda render). Faz 1: sade tasarım.

public class VehicleController : Controller
{
    private readonly IPublicVehicleService _publicVehicleService;

    public VehicleController(IPublicVehicleService publicVehicleService)
    {
        _publicVehicleService = publicVehicleService;
    }

    /// <summary>
    /// /arac/{slug}-{ilanNo}. Araç ilan no ile bulunur; adresteki slug güncel değilse 301.
    /// </summary>
    [HttpGet("arac/{slugAndNo}")]
    public async Task<IActionResult> Detail(string slugAndNo)
    {
        if (!SlugHelper.TryParseVehiclePath(slugAndNo, out var slug, out var listingNo))
            return NotFound();

        var vehicle = await _publicVehicleService.GetVehicleAsync(listingNo);
        if (vehicle == null)
            return NotFound();

        if (!string.Equals(slug, vehicle.Slug, StringComparison.Ordinal))
            return RedirectPermanent(vehicle.Url);

        return View(vehicle);
    }
}
