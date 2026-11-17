using FatihTasci.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using FatihTasci.Core.Models.Domain.Catalog;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Api;

[Route("api/features")]
public class FeaturesController : BaseApiController
{
    private readonly IFeatureService _featureService;

    public FeaturesController(IFeatureService featureService)
    {
        _featureService = featureService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        return Ok(await _featureService.GetGroupedAsync(includeInactive));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveFeatureRequestModel request)
    {
        return Ok(await _featureService.CreateAsync(request));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveFeatureRequestModel request)
    {
        return Ok(await _featureService.UpdateAsync(id, request));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut("order")]
    public async Task<IActionResult> Reorder([FromBody] FeatureOrderRequestModel request)
    {
        return Ok(await _featureService.ReorderAsync(request));
    }
}
