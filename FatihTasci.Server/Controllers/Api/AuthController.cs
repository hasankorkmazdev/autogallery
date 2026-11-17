using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Account;
using FatihTasci.Core.Providers;
using FatihTasci.Core.Service.Domains.Abstracts;
using FatihTasci.Server.Helpers;
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

    /// <summary>
    /// Başarılı girişte token HttpOnly çereze yazılır; gövdede yalnız kullanıcı ve bitiş zamanı döner.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting(Program.LoginRateLimitPolicy)]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
    {
        var result = await _accountService.LoginAsync(request);
        if (result.Data != null)
            Response.Cookies.Append(AuthCookie.Name, result.Data.Token, AuthCookie.Options(result.Data.ExpiresAt));

        return Ok(result);
    }

    /// <summary>
    /// Oturum çerezini siler. Çerez HttpOnly olduğu için panel kendisi silemez.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookie.Name, AuthCookie.Options());
        return Ok(new ServiceResultModel { Message = "Çıkış yapıldı." });
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        return Ok(await _accountService.GetUserAsync(_currentUserProvider.GetUserId().Value));
    }
}
