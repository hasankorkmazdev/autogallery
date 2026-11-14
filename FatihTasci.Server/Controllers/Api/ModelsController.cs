using FatihTasci.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using FatihTasci.Core.Models.Domain.Catalog;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Api;

[Route("api/models")]
public class ModelsController : BaseApiController
{
    private readonly ICatalogService _catalogService;

    public ModelsController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveCarModelRequestModel request)
    {
        return Ok(await _catalogService.CreateModelAsync(request));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveCarModelRequestModel request)
    {
        return Ok(await _catalogService.UpdateModelAsync(id, request));
    }
}
