using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Lead;
using FatihTasci.Core.Models.Domain.Setting;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Api;

[Route("api/leads")]
public class LeadsController : BaseApiController
{
    private readonly ILeadService _leadService;

    public LeadsController(ILeadService leadService)
    {
        _leadService = leadService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] LeadListRequestModel request)
    {
        return Ok(await _leadService.GetListAsync(request));
    }

    [HttpGet("new-count")]
    public async Task<IActionResult> GetNewCount()
    {
        return Ok(await _leadService.GetNewCountAsync());
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeadRequestModel request)
    {
        return Ok(await _leadService.UpdateAsync(id, request));
    }
}

[Route("api/settings")]
public class SettingsController : BaseApiController
{
    private readonly ISiteSettingService _settingService;

    public SettingsController(ISiteSettingService settingService)
    {
        _settingService = settingService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(new ServiceResultModel<SiteSettingsModel> { Data = await _settingService.GetAsync(), Message = "İşlem başarılı." });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] SiteSettingsModel request)
    {
        return Ok(await _settingService.UpdateAsync(request));
    }
}
