using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Api;

/// <summary>
/// Araca atanabilecek danışmanlar. Personel de araç düzenlediği için UsersController'dan (yalnız Yönetici) ayrıdır.
/// </summary>
[Route("api/advisors")]
public class AdvisorsController : BaseApiController
{
    private readonly IUserService _userService;

    public AdvisorsController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _userService.GetAdvisorsAsync());
    }
}
