using Microsoft.AspNetCore.Http;

namespace FatihTasci.Core.Providers;

/// <summary>
/// Oturumdaki panel kullanıcısını verir. Signage'daki ITenantProvider'ın tenant'sız karşılığı.
/// </summary>
public interface ICurrentUserProvider
{
    /// <summary>
    /// Giriş yapılmamışsa (vitrin istekleri, seed) null döner.
    /// </summary>
    Guid? GetUserId();
}

public class HttpContextCurrentUserProvider : ICurrentUserProvider
{
    public const string UserIdClaim = "UserId";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        var claim = user.FindFirst(UserIdClaim);
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
