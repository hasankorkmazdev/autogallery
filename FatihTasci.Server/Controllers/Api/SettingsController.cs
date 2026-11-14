using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Setting;
using FatihTasci.Core.Service.Domains;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Api;

/// <summary>
/// Site ayarları. Okuma tüm panel kullanıcılarına açıktır (panel başlığında firma adı ve logo),
/// değiştirme yalnız Yönetici'ye.
/// </summary>
[Route("api/settings")]
public class SettingsController : BaseApiController
{
    private readonly ISiteSettingService _settingService;
    private readonly IMediaRegenerationRunner _regenerationRunner;

    public SettingsController(ISiteSettingService settingService, IMediaRegenerationRunner regenerationRunner)
    {
        _settingService = settingService;
        _regenerationRunner = regenerationRunner;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(new ServiceResultModel<SiteSettingsModel> { Data = await _settingService.GetAsync(), Message = "İşlem başarılı." });
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] SiteSettingsModel request)
    {
        return Ok(await _settingService.UpdateAsync(request));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost("logo")]
    [RequestSizeLimit(SiteSettingService.MaxLogoBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = SiteSettingService.MaxLogoBytes + 1024 * 1024)]
    public async Task<IActionResult> UploadLogo(IFormFile file)
    {
        return Ok(await _settingService.UploadLogoAsync(file));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpDelete("logo")]
    public async Task<IActionResult> DeleteLogo()
    {
        return Ok(await _settingService.DeleteLogoAsync());
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet("watermark/status")]
    public IActionResult GetWatermarkStatus()
    {
        return Ok(new ServiceResultModel<WatermarkJobStatusModel> { Data = _regenerationRunner.GetStatus(), Message = "İşlem başarılı." });
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost("watermark/apply")]
    public IActionResult ApplyWatermark()
    {
        var started = _regenerationRunner.TryStart();
        return Ok(new ServiceResultModel<WatermarkJobStatusModel>
        {
            Data = _regenerationRunner.GetStatus(),
            Message = started
                ? "Fotoğraflar arka planda yeniden işleniyor."
                : "Filigran işi zaten sürüyor."
        });
    }
}
