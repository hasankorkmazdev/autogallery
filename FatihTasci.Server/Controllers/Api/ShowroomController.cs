using FatihTasci.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using FatihTasci.Core.Models.Domain.Showroom;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Api;

/// <summary>
/// Ana sayfa "Galeri Atmosferi" sahneleri. Her işlem güncel sahne listesini döner.
/// </summary>
[Route("api/showroom-scenes")]
[Authorize(Roles = RoleNames.Admin)]
public class ShowroomController : BaseApiController
{
    private readonly IShowroomService _showroomService;

    public ShowroomController(IShowroomService showroomService)
    {
        _showroomService = showroomService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _showroomService.GetAllAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveShowroomSceneRequestModel request)
    {
        return Ok(await _showroomService.CreateAsync(request));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveShowroomSceneRequestModel request)
    {
        return Ok(await _showroomService.UpdateAsync(id, request));
    }

    [HttpPut("order")]
    public async Task<IActionResult> Reorder([FromBody] ShowroomSceneOrderRequestModel request)
    {
        return Ok(await _showroomService.ReorderAsync(request));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return Ok(await _showroomService.DeleteAsync(id));
    }

    [HttpPost("{id:int}/image")]
    [RequestSizeLimit(Program.MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = Program.MaxUploadBytes)]
    public async Task<IActionResult> UploadImage(int id, IFormFile file)
    {
        return Ok(await _showroomService.UploadImageAsync(id, file));
    }

    [HttpDelete("{id:int}/image")]
    public async Task<IActionResult> DeleteImage(int id)
    {
        return Ok(await _showroomService.DeleteImageAsync(id));
    }
}
