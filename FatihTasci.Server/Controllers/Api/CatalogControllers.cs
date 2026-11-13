using FatihTasci.Core.Enums;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain;
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveCarModelRequestModel request)
    {
        return Ok(await _catalogService.UpdateModelAsync(id, request));
    }
}

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

[Route("api/features")]
public class FeaturesController : BaseApiController
{
    private readonly IFeatureService _featureService;

    public FeaturesController(IFeatureService featureService)
    {
        _featureService = featureService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        return Ok(await _featureService.GetGroupedAsync(includeInactive));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveFeatureRequestModel request)
    {
        return Ok(await _featureService.CreateAsync(request));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveFeatureRequestModel request)
    {
        return Ok(await _featureService.UpdateAsync(id, request));
    }
}

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
            ["leadStatus"] = EnumHelper.GetOptions<LeadStatus>()
        };

        return Ok(new ServiceResultModel<Dictionary<string, List<EnumOption>>> { Data = lookups, Message = "İşlem başarılı." });
    }
}
