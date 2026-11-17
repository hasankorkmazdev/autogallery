using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Site;

/// <summary>
/// Kurumsal sayfa (/kurumsal): giriş, Sayılarla Biz, Hakkımızda, Vizyon ve Misyon. Yazılar panelde Site İçeriği'nden gelir.
/// </summary>
public class CorporateController : Controller
{
    private readonly ISiteContentService _contentService;

    public CorporateController(ISiteContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet("kurumsal")]
    public async Task<IActionResult> Index()
    {
        return View(await _contentService.GetAsync());
    }
}
