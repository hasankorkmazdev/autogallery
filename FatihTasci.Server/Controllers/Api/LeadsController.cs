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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeadRequestModel request)
    {
        return Ok(await _leadService.UpdateAsync(id, request));
    }
}
