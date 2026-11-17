using FatihTasci.Core.Models.Domain.Delivery;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Api;

/// <summary>
/// Teslimatlar (Personel ve Yönetici). Ekleme ve düzenleme multipart'tır: alanlar + "photo". Her işlem güncel listeyi döner.
/// </summary>
[Route("api/deliveries")]
public class DeliveriesController : BaseApiController
{
    private readonly IDeliveryService _deliveryService;

    public DeliveriesController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _deliveryService.GetAllAsync());
    }

    [HttpPost]
    [RequestSizeLimit(Program.MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = Program.MaxUploadBytes)]
    public async Task<IActionResult> Create([FromForm] SaveDeliveryRequestModel request, IFormFile photo)
    {
        return Ok(await _deliveryService.CreateAsync(request, photo));
    }

    [HttpPut("{id:int}")]
    [RequestSizeLimit(Program.MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = Program.MaxUploadBytes)]
    public async Task<IActionResult> Update(int id, [FromForm] SaveDeliveryRequestModel request, IFormFile photo)
    {
        return Ok(await _deliveryService.UpdateAsync(id, request, photo));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return Ok(await _deliveryService.DeleteAsync(id));
    }
}
