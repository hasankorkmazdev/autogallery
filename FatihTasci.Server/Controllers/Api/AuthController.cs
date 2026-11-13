using FatihTasci.Core.Models.Domain.Account;
using FatihTasci.Core.Providers;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FatihTasci.Server.Controllers.Api;

public class AuthController : BaseApiController
{
    private readonly IAccountService _accountService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public AuthController(IAccountService accountService, ICurrentUserProvider currentUserProvider)
    {
        _accountService = accountService;
        _currentUserProvider = currentUserProvider;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting(Program.LoginRateLimitPolicy)]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
    {
        return Ok(await _accountService.LoginAsync(request));
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        return Ok(await _accountService.GetUserAsync(_currentUserProvider.GetUserId().Value));
    }
}
