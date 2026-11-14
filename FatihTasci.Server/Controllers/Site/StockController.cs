using FatihTasci.Core.Models.Domain.Public;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Site;

// Vitrin sayfaları (Razor, sunucuda render). Faz 1: sade tasarım.

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
