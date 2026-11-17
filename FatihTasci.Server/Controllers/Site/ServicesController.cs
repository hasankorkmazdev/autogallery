using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Content;
using FatihTasci.Core.Models.Domain.Lead;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FatihTasci.Server.Controllers.Site;

/// <summary>
/// Hizmetler: liste (/hizmetler) ve her hizmetin sayfası (/hizmetler/{slug}). Yazılar panelde Site İçeriği'nden gelir.
/// "Aracımı Satmak İstiyorum" sayfasında fotoğraflı teklif formu vardır; form aynı adrese gönderilir.
/// </summary>
public class ServicesController : Controller
{
    // Teklif formu isteği: fotoğraflar + form alanları için 1 MB pay. Genel yükleme sınırından (panel) düşüktür.
    private const long SellFormMaxBytes = CreateSellLeadRequestModel.MaxPhotos * CreateSellLeadRequestModel.MaxPhotoBytes + 1024 * 1024;

    private readonly ISiteContentService _contentService;
    private readonly ICatalogService _catalogService;
    private readonly ILeadService _leadService;

    public ServicesController(ISiteContentService contentService, ICatalogService catalogService, ILeadService leadService)
    {
        _contentService = contentService;
        _catalogService = catalogService;
        _leadService = leadService;
    }

    [HttpGet("hizmetler")]
    public async Task<IActionResult> Index()
    {
        var content = await _contentService.GetAsync();
        return View(content.Services);
    }

    [HttpGet("hizmetler/{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        var page = await GetPageAsync(slug);
        if (page == null)
            return NotFound();

        if (page.Service.Type == SiteServiceType.SellVehicle)
            await PrepareSellFormAsync(page, new CreateSellLeadRequestModel());

        return View(page);
    }

    /// <summary>
    /// "Aracımı Satmak İstiyorum" teklif formu. Hata varsa sayfa girilen bilgilerle yeniden gösterilir
    /// (tarayıcı seçilen dosyaları geri dolduramaz, ziyaretçiye fotoğrafları yeniden seçmesi söylenir).
    /// </summary>
    [HttpPost("hizmetler/{slug}")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(Program.FormRateLimitPolicy)]
    [RequestSizeLimit(SellFormMaxBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = SellFormMaxBytes)]
    public async Task<IActionResult> Detail(string slug, CreateSellLeadRequestModel request)
    {
        var page = await GetPageAsync(slug);
        if (page == null || page.Service.Type != SiteServiceType.SellVehicle)
            return NotFound();

        if (ModelState.IsValid)
        {
            var result = await _leadService.CreateSellAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString());
            if (result.Code == StatusCodes.Status200OK)
            {
                TempData["FormSuccess"] = result.Message;
                return Redirect(page.Service.Path + "#teklif");
            }

            ModelState.AddModelError(nameof(request.Photos), result.Message);
        }

        ViewData["PhotosLost"] = request.Photos?.Count > 0;
        request.Photos = new List<IFormFile>();
        await PrepareSellFormAsync(page, request);
        return View(page);
    }

    private async Task<ServicePageModel> GetPageAsync(string slug)
    {
        var content = await _contentService.GetAsync();
        var service = content.Services.FirstOrDefault(x => x.Path == $"/hizmetler/{slug}");
        if (service == null)
            return null;

        return new ServicePageModel
        {
            Service = service,
            Others = content.Services.Where(x => x.Type != service.Type).ToList()
        };
    }

    private async Task PrepareSellFormAsync(ServicePageModel page, CreateSellLeadRequestModel form)
    {
        page.SellForm = form;
        var brands = await _catalogService.GetBrandsAsync(includeInactive: false);
        page.SellBrands = brands.Data?.Select(x => x.Name).ToList() ?? new List<string>();
    }
}
