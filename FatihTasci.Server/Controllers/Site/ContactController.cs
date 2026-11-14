using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Lead;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FatihTasci.Server.Controllers.Site;

// Vitrin sayfaları (Razor, sunucuda render). Faz 1: sade tasarım.

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
