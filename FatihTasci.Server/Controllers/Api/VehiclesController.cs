using FatihTasci.Core.Models.Domain.Vehicle;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Api;

/// <summary>
/// Araç listesi ve 7 adımlı sihirbaz. Her adım kendi uç noktasıyla taslağa kaydedilir.
/// </summary>
[Route("api/vehicles")]
public class VehiclesController : BaseApiController
{
    private readonly IVehicleService _vehicleService;
    private readonly IMediaService _mediaService;

    public VehiclesController(IVehicleService vehicleService, IMediaService mediaService)
    {
        _vehicleService = vehicleService;
        _mediaService = mediaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] VehicleListRequestModel request)
    {
        return Ok(await _vehicleService.GetListAsync(request));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        return Ok(await _vehicleService.GetDetailAsync(id));
    }

    /// <summary>
    /// Adım 1 ile taslak oluşturur; id ve ilan no döner.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VehicleIdentityRequestModel request)
    {
        return Ok(await _vehicleService.CreateAsync(request));
    }

    [HttpPut("{id:guid}/identity")]
    public async Task<IActionResult> UpdateIdentity(Guid id, [FromBody] VehicleIdentityRequestModel request)
    {
        return Ok(await _vehicleService.UpdateIdentityAsync(id, request));
    }

    [HttpPut("{id:guid}/technical")]
    public async Task<IActionResult> UpdateTechnical(Guid id, [FromBody] VehicleTechnicalRequestModel request)
    {
        return Ok(await _vehicleService.UpdateTechnicalAsync(id, request));
    }

    [HttpPut("{id:guid}/condition")]
    public async Task<IActionResult> UpdateCondition(Guid id, [FromBody] VehicleConditionRequestModel request)
    {
        return Ok(await _vehicleService.UpdateConditionAsync(id, request));
    }

    [HttpPut("{id:guid}/body-parts")]
    public async Task<IActionResult> UpdateBodyParts(Guid id, [FromBody] VehicleBodyPartsRequestModel request)
    {
        return Ok(await _vehicleService.UpdateBodyPartsAsync(id, request));
    }

    [HttpPut("{id:guid}/features")]
    public async Task<IActionResult> UpdateFeatures(Guid id, [FromBody] VehicleFeaturesRequestModel request)
    {
        return Ok(await _vehicleService.UpdateFeaturesAsync(id, request));
    }

    [HttpGet("{id:guid}/features/suggestion")]
    public async Task<IActionResult> GetFeatureSuggestion(Guid id)
    {
        return Ok(await _vehicleService.GetFeatureSuggestionAsync(id));
    }

    [HttpPost("{id:guid}/media")]
    [RequestSizeLimit(Program.MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = Program.MaxUploadBytes)]
    public async Task<IActionResult> UploadMedia(Guid id)
    {
        var form = await Request.ReadFormAsync();
        return Ok(await _mediaService.UploadVehicleImagesAsync(id, form.Files));
    }

    [HttpPut("{id:guid}/media/order")]
    public async Task<IActionResult> ReorderMedia(Guid id, [FromBody] VehicleMediaOrderRequestModel request)
    {
        return Ok(await _mediaService.ReorderAsync(id, request));
    }

    [HttpPut("{id:guid}/media/{mediaId:guid}/cover")]
    public async Task<IActionResult> SetCover(Guid id, Guid mediaId)
    {
        return Ok(await _mediaService.SetCoverAsync(id, mediaId));
    }

    [HttpDelete("{id:guid}/media/{mediaId:guid}")]
    public async Task<IActionResult> DeleteMedia(Guid id, Guid mediaId)
    {
        return Ok(await _mediaService.DeleteAsync(id, mediaId));
    }

    [HttpPut("{id:guid}/video")]
    public async Task<IActionResult> UpdateVideo(Guid id, [FromBody] VehicleVideoRequestModel request)
    {
        return Ok(await _vehicleService.UpdateVideoAsync(id, request));
    }

    [HttpPost("{id:guid}/expertise")]
    [RequestSizeLimit(Program.MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = Program.MaxUploadBytes)]
    public async Task<IActionResult> UploadExpertise(Guid id, IFormFile file)
    {
        return Ok(await _mediaService.UploadExpertiseReportAsync(id, file));
    }

    [HttpDelete("{id:guid}/expertise")]
    public async Task<IActionResult> DeleteExpertise(Guid id)
    {
        return Ok(await _mediaService.DeleteExpertiseReportAsync(id));
    }

    [HttpPut("{id:guid}/pricing")]
    public async Task<IActionResult> UpdatePricing(Guid id, [FromBody] VehiclePricingRequestModel request)
    {
        return Ok(await _vehicleService.UpdatePricingAsync(id, request));
    }

    [HttpGet("{id:guid}/publish-check")]
    public async Task<IActionResult> PublishCheck(Guid id)
    {
        return Ok(await _vehicleService.GetPublishCheckAsync(id));
    }

    /// <summary>
    /// Yayınla / Rezerve / Satıldı / Arşiv / Taslak. Yayınlarken kontrol listesi zorunludur.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] VehicleStatusRequestModel request)
    {
        return Ok(await _vehicleService.UpdateStatusAsync(id, request));
    }

    [HttpPost("{id:guid}/copy")]
    public async Task<IActionResult> Copy(Guid id)
    {
        return Ok(await _vehicleService.CopyAsync(id));
    }
}
