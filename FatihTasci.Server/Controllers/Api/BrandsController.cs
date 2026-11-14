using FatihTasci.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using FatihTasci.Core.Models.Domain.Catalog;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Api;

[Route("api/brands")]
public class BrandsController : BaseApiController
{
    private readonly ICatalogService _catalogService;

    public BrandsController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        return Ok(await _catalogService.GetBrandsAsync(includeInactive));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveBrandRequestModel request)
    {
        return Ok(await _catalogService.CreateBrandAsync(request));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveBrandRequestModel request)
    {
        return Ok(await _catalogService.UpdateBrandAsync(id, request));
    }

    [HttpGet("{id:int}/series")]
    public async Task<IActionResult> GetSeries(int id, [FromQuery] bool includeInactive = false)
    {
        return Ok(await _catalogService.GetSeriesAsync(id, includeInactive));
    }
}
