using FatihTasci.Core.Enums;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain.Lead;
using FatihTasci.Core.Models.Domain.Public;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FatihTasci.Server.Controllers.Site;

// Vitrin sayfaları (Razor, sunucuda render). Faz 1: sade tasarım.

public class HomeController : Controller
{
    private readonly IPublicVehicleService _publicVehicleService;

    public HomeController(IPublicVehicleService publicVehicleService)
    {
        _publicVehicleService = publicVehicleService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return View(await _publicVehicleService.GetHomeAsync());
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

public class StockController : Controller
{
    private readonly IPublicVehicleService _publicVehicleService;

    public StockController(IPublicVehicleService publicVehicleService)
    {
        _publicVehicleService = publicVehicleService;
    }

    /// <summary>
    /// /stok, /stok/{marka}, /stok/{marka}/{seri}. Diğer filtreler query string'de.
    /// Eski marka / seri slug'ı ile gelinirse güncel adrese 301 ile yönlendirilir.
    /// </summary>
    [HttpGet("stok/{brand?}/{series?}")]
    public async Task<IActionResult> Index(string brand, string series, [FromQuery] StockFilterModel filter)
    {
        // Filtre formu marka / seriyi query string ile gönderir; SEO için adres yoluna taşınır.
        if (Request.Query.ContainsKey("brand") || Request.Query.ContainsKey("series"))
            return RedirectToCanonicalStockPath();

        var path = await _publicVehicleService.ResolveCatalogPathAsync(brand, series);
        if (!path.Found)
            return NotFound();

        if (path.RedirectPath != null)
            return RedirectPermanent(path.RedirectPath + Request.QueryString);

        filter.Brand = path.BrandSlug;
        filter.Series = path.SeriesSlug;
        ViewBag.FilterOptions = await _publicVehicleService.GetFilterOptionsAsync();
        return View(await _publicVehicleService.GetStockAsync(filter, path));
    }

    private IActionResult RedirectToCanonicalStockPath()
    {
        var brand = Request.Query["brand"].ToString();
        var series = Request.Query["series"].ToString();
        var target = "/stok";
        if (!string.IsNullOrWhiteSpace(brand))
        {
            target += $"/{Uri.EscapeDataString(brand)}";
            if (!string.IsNullOrWhiteSpace(series))
                target += $"/{Uri.EscapeDataString(series)}";
        }

        var rest = Request.Query
            .Where(x => x.Key != "brand" && x.Key != "series" && x.Key != "page" && !string.IsNullOrEmpty(x.Value))
            .SelectMany(x => x.Value.Select(v => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(v)}"));
        var query = string.Join("&", rest);

        return Redirect(string.IsNullOrEmpty(query) ? target : $"{target}?{query}");
    }
}

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

public class ContactController : Controller
{
    private readonly ILeadService _leadService;

    public ContactController(ILeadService leadService)
    {
        _leadService = leadService;
    }

    [HttpGet("iletisim")]
    public IActionResult Index()
    {
        return View(new CreateLeadRequestModel { Type = LeadType.Contact });
    }

    [HttpPost("iletisim")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(Program.FormRateLimitPolicy)]
    public async Task<IActionResult> Index(CreateLeadRequestModel request)
    {
        request.Type = LeadType.Contact;
        request.ListingNo = null;
        if (!ModelState.IsValid)
            return View(request);

        var result = await _leadService.CreateAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["FormSuccess"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Araç sayfasındaki "Bilgi al" ve "Test sürüşü" formları. Sonuç araç sayfasında gösterilir.
    /// </summary>
    [HttpPost("talep")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(Program.FormRateLimitPolicy)]
    public async Task<IActionResult> VehicleLead(CreateLeadRequestModel request, string returnUrl)
    {
        if (request.Type != LeadType.Information && request.Type != LeadType.TestDrive)
            ModelState.AddModelError(nameof(request.Type), "Talep türü geçersiz.");

        if (ModelState.IsValid)
        {
            var result = await _leadService.CreateAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["FormSuccess"] = result.Message;
        }
        else
        {
            TempData["FormError"] = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).FirstOrDefault();
        }

        var target = Url.IsLocalUrl(returnUrl) ? returnUrl : "/stok";
        return Redirect(target + "#talep");
    }
}
