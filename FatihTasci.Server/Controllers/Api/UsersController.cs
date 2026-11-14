using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain.Account;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Api;

[Route("api/users")]
[Authorize(Roles = RoleNames.Admin)]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        return Ok(await _userService.GetListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveUserRequestModel request)
    {
        return Ok(await _userService.CreateAsync(request));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveUserRequestModel request)
    {
        return Ok(await _userService.UpdateAsync(id, request));
    }

    [HttpPut("{id:guid}/password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetUserPasswordRequestModel request)
    {
        return Ok(await _userService.ResetPasswordAsync(id, request));
    }
}
