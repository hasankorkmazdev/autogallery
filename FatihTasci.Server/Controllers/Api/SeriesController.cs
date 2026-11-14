using FatihTasci.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using FatihTasci.Core.Models.Domain.Catalog;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Api;

[Route("api/series")]
public class SeriesController : BaseApiController
{
    private readonly ICatalogService _catalogService;

    public SeriesController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveSeriesRequestModel request)
    {
        return Ok(await _catalogService.CreateSeriesAsync(request));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveSeriesRequestModel request)
    {
        return Ok(await _catalogService.UpdateSeriesAsync(id, request));
    }

    [HttpGet("{id:int}/models")]
    public async Task<IActionResult> GetModels(int id, [FromQuery] bool includeInactive = false)
    {
        return Ok(await _catalogService.GetModelsAsync(id, includeInactive));
    }
}
