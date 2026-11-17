using FatihTasci.Core.Models.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace FatihTasci.Core.Middleware;

/// <summary>
/// Güvenlik başlıkları (CSP vb.) ve API için CSRF kontrolü.
/// </summary>
public static class SecurityMiddlewareExtensions
{
    /// <summary>
    /// Panel her API isteğinde bu başlığı gönderir. Başka bir site bu başlığı CORS ön kontrolü
    /// olmadan ekleyemez ve sunucuda CORS açık olmadığından istek tarayıcıda durur.
    /// </summary>
    public const string CsrfHeaderName = "X-Requested-With";
    public const string CsrfHeaderValue = "XMLHttpRequest";

    private const string ApiPolicy = "default-src 'none'; frame-ancestors 'none'";

    // Radix / sonner çalışma anında <style> ekler, bu yüzden style-src 'unsafe-inline'. Script'te satır içi yok.
    private const string PanelPolicy =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: blob:; font-src 'self' data:; connect-src 'self'; media-src 'self' blob:; " +
        "object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";

    // Vitrinde dış kaynak yok (KVKK); yalnız YouTube nocookie gömmesi ve ziyaretçi isteyince yüklenen
    // Google Haritalar gömmesi (_Location.cshtml, site.js data-map). img-src blob: teklif formunda seçilen
    // fotoğrafların önizlemesi içindir (site.js data-sell-form; dosya tarayıcıdan çıkmaz).
    private const string SitePolicy =
        "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data: blob:; font-src 'self'; " +
        "connect-src 'self'; frame-src https://www.youtube-nocookie.com https://www.google.com; " +
        "object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'self'";

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        var isDevelopment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();

        return app.Use(async (context, next) =>
        {
            // Hata sayfaları yanıtı temizlediği için başlıklar yanıt başlarken yazılır.
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers.XContentTypeOptions = "nosniff";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

                // Geliştirici hata sayfası satır içi stil / script kullanır.
                if (isDevelopment && context.Response.StatusCode >= 500)
                    return Task.CompletedTask;

                var path = context.Request.Path;
                var policy = GetPolicy(path);
                if (policy != null)
                    headers.ContentSecurityPolicy = policy;

                // frame-ancestors'u tanımayan eski tarayıcılar için
                if (path.StartsWithSegments("/api") || path.StartsWithSegments("/panel"))
                    headers.XFrameOptions = "DENY";

                return Task.CompletedTask;
            });

            await next(context);
        });
    }

    /// <summary>
    /// /api altındaki durum değiştiren isteklerde (POST, PUT, DELETE...) CSRF başlığını zorunlu tutar.
    /// </summary>
    public static IApplicationBuilder UseApiCsrfProtection(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var request = context.Request;
            var isSafeMethod = HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method) || HttpMethods.IsOptions(request.Method);

            if (request.Path.StartsWithSegments("/api") && !isSafeMethod &&
                !string.Equals(request.Headers[CsrfHeaderName], CsrfHeaderValue, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new ServiceResultModel
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = "İstek doğrulanamadı. Sayfayı yenileyip tekrar deneyin."
                }));
                return;
            }

            await next(context);
        });
    }

    private static string GetPolicy(PathString path)
    {
        if (path.StartsWithSegments("/api"))
            return ApiPolicy;
        if (path.StartsWithSegments("/panel"))
            return PanelPolicy;
        // Ekspertiz PDF'leri tarayıcının PDF görüntüleyicisinde açılır; CSP görüntüleyiciyi bozabilir.
        if (path.StartsWithSegments("/media"))
            return null;
        return SitePolicy;
    }
}
