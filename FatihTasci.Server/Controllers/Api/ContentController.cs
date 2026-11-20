using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Content;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Api;

/// <summary>
/// Vitrin yazıları (Hizmetler, Kurumsal, Sayılarla Biz). Site ayarları gibi yalnız Yönetici'ye açıktır.
/// </summary>
[Route("api/content")]
[Authorize(Roles = RoleNames.Admin)]
public class ContentController : BaseApiController
{
    private readonly ISiteContentService _contentService;

    public ContentController(ISiteContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(new ServiceResultModel<SiteContentModel> { Data = await _contentService.GetAsync(), Message = "İşlem başarılı." });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] SiteContentModel request)
    {
        return Ok(await _contentService.UpdateAsync(request));
    }

    /// <summary>
    /// Ana sayfa videosu (MP4) ve panelin videodan aldığı ilk kare (afiş).
    /// </summary>
    [HttpPost("hero-video")]
    [RequestSizeLimit(VideoUpload.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = VideoUpload.MaxRequestBytes)]
    public async Task<IActionResult> UploadHeroVideo(IFormFile video, IFormFile poster)
    {
        return Ok(await _contentService.UploadHeroVideoAsync(video, poster));
    }

    /// <summary>
    /// Yüklenen videoyu siler, varsayılan drone videosuna döner.
    /// </summary>
    [HttpDelete("hero-video")]
    public async Task<IActionResult> DeleteHeroVideo()
    {
        return Ok(await _contentService.DeleteHeroVideoAsync());
    }
}
