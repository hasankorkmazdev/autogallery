using FatihTasci.Core.Models.Domain.Public;
using FatihTasci.Core.Service.Domains.Abstracts;
using FatihTasci.Server.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Site;

/// <summary>
/// 1200×630 paylaşım görselleri (og:image). İlk istekte üretilir, sonra önbellekten verilir.
/// Adresteki ?v= güncel sürümse bir yıl önbelleğe alınır; içerik (fiyat, başlık, fotoğraf) değişince sayfadaki adres de değişir.
/// </summary>
public class ShareImageController : Controller
{
    private readonly IPublicVehicleService _publicVehicleService;
    private readonly IShowroomService _showroomService;
    private readonly ISiteSettingService _settingService;
    private readonly IShareImageService _shareImageService;

    public ShareImageController(
        IPublicVehicleService publicVehicleService,
        IShowroomService showroomService,
        ISiteSettingService settingService,
        IShareImageService shareImageService)
    {
        _publicVehicleService = publicVehicleService;
        _showroomService = showroomService;
        _settingService = settingService;
        _shareImageService = shareImageService;
    }

    [HttpGet("paylasim/arac/{listingNo:int}.jpg")]
    public async Task<IActionResult> Vehicle(int listingNo, string v)
    {
        var vehicle = await _publicVehicleService.GetShareAsync(listingNo);
        if (vehicle == null)
            return NotFound();

        var settings = await _settingService.GetAsync();
        return await ServeAsync(ShareCards.ForVehicle(vehicle, settings), v);
    }

    /// <summary>
    /// Ana sayfa, ilanlar ve diğer sayfalar: galeri adı ve logo; zemin ilk showroom sahnesi,
    /// sahne yoksa öne çıkan (yoksa en yeni) aracın kapak fotoğrafı.
    /// </summary>
    [HttpGet("paylasim/galeri.jpg")]
    public async Task<IActionResult> Site(string v)
    {
        var settings = await _settingService.GetAsync();
        var background = (await _showroomService.GetPublicAsync()).FirstOrDefault()?.LargeUrl;
        if (background == null)
        {
            var home = await _publicVehicleService.GetHomeAsync();
            background = home.Showcase.Concat(home.Latest).Select(x => x.CoverLargeUrl ?? x.CoverUrl).FirstOrDefault(x => x != null);
        }
        return await ServeAsync(ShareCards.ForSite(settings, background), v);
    }

    private async Task<IActionResult> ServeAsync(ShareCardModel card, string version)
    {
        var path = await _shareImageService.GetOrCreateAsync(card);
        Response.Headers.CacheControl = version == _shareImageService.ComputeVersion(card)
            ? "public, max-age=31536000, immutable"
            : "public, max-age=3600";
        return PhysicalFile(path, "image/jpeg");
    }
}
