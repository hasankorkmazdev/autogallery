using FatihTasci.Core.Enums;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Api;

/// <summary>
/// Paneldeki seçim listeleri için enum seçenekleri (Türkçe adlarıyla).
/// </summary>
[Route("api/lookups")]
public class LookupsController : BaseApiController
{
    [HttpGet]
    public IActionResult Get()
    {
        var lookups = new Dictionary<string, List<EnumOption>>
        {
            ["vehicleStatus"] = EnumHelper.GetOptions<VehicleStatus>(),
            ["fuelType"] = EnumHelper.GetOptions<FuelType>(),
            ["transmission"] = EnumHelper.GetOptions<TransmissionType>(),
            ["bodyType"] = EnumHelper.GetOptions<BodyType>(),
            ["drivetrain"] = EnumHelper.GetOptions<Drivetrain>(),
            ["condition"] = EnumHelper.GetOptions<VehicleCondition>(),
            ["plateOrigin"] = EnumHelper.GetOptions<PlateOrigin>(),
            ["bodyPart"] = EnumHelper.GetOptions<BodyPart>(),
            ["bodyPartState"] = EnumHelper.GetOptions<BodyPartState>(),
            ["featureCategory"] = EnumHelper.GetOptions<FeatureCategory>(),
            ["leadType"] = EnumHelper.GetOptions<LeadType>(),
            ["leadStatus"] = EnumHelper.GetOptions<LeadStatus>(),
            ["userRole"] = EnumHelper.GetOptions<UserRole>()
        };

        return Ok(new ServiceResultModel<Dictionary<string, List<EnumOption>>> { Data = lookups, Message = "İşlem başarılı." });
    }
}
