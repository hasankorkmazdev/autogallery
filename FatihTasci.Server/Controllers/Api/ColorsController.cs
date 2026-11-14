using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Api;

[Route("api/colors")]
public class ColorsController : BaseApiController
{
    private readonly ICatalogService _catalogService;

    public ColorsController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _catalogService.GetColorsAsync());
    }
}
