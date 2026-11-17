namespace FatihTasci.Server.Helpers;

/// <summary>
/// Panel oturumunun JWT'si HttpOnly çerezde taşınır; localStorage'a yazılmaz.
/// __Host- ön eki: tarayıcı çerezi yalnız Secure, Path=/ ve Domain'siz olarak kabul eder
/// (alt alan adından ya da şifresiz bağlantıdan üzerine yazılamaz).
/// </summary>
public static class AuthCookie
{
    public const string Name = "__Host-access_token";

    public static CookieOptions Options(DateTime? expiresAt = null)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            IsEssential = true,
            Expires = expiresAt.HasValue ? new DateTimeOffset(expiresAt.Value) : null
        };
    }
}
