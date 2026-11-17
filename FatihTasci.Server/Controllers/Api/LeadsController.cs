using FatihTasci.Core.Models.Domain.Lead;
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

    /// <summary>
    /// "Aracını Sat" talebinin fotoğrafı. Dosya özel klasörde durur, yalnız oturum açmış personel alır;
    /// tarayıcı panel kapandıktan sonra uzun süre saklamasın diye önbellek kısa ve özeldir.
    /// </summary>
    [HttpGet("{id:guid}/media/{mediaId:guid}")]
    public async Task<IActionResult> GetMedia(Guid id, Guid mediaId)
    {
        var result = await _leadService.GetMediaFileAsync(id, mediaId);
        if (result.Data == null)
            return Ok(result);

        Response.Headers.CacheControl = "private, max-age=3600";
        return PhysicalFile(result.Data, "image/webp");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeadRequestModel request)
    {
        return Ok(await _leadService.UpdateAsync(id, request));
    }
}
