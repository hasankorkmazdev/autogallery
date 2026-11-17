namespace FatihTasci.Server.Helpers;

/// <summary>
/// Vitrin için tam adres üretir (canonical, og:url, og:image, sitemap, schema.org).
/// Site:BaseUrl doluysa o kullanılır (ters vekil arkasında doğru alan adı için); boşsa isteğin adresi.
/// </summary>
public class SiteUrlBuilder
{
    private readonly string _baseUrl;

    public SiteUrlBuilder(IConfiguration configuration)
    {
        _baseUrl = configuration.GetValue<string>("Site:BaseUrl")?.Trim().TrimEnd('/');
    }

    public string Base(HttpRequest request)
    {
        return string.IsNullOrEmpty(_baseUrl) ? $"{request.Scheme}://{request.Host}" : _baseUrl;
    }

    /// <summary>
    /// /arac/... -> https://alan-adi/arac/... (zaten tam adresse aynen döner)
    /// </summary>
    public string Absolute(HttpRequest request, string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return path;

        return Base(request) + (path.StartsWith('/') ? path : "/" + path);
    }
}
