using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace FatihTasci.Core.Logging;

/// <summary>
/// Server için Serilog kurulumu (Signage'daki AddGoraSerilog'un sadeleştirilmiş hali).
///   logs/app/app-*.log       -> tüm uygulama kayıtları (compact JSON)
///   logs/errors/error-*.log  -> yalnızca Error ve üzeri
/// Konsol renkli, okunabilir metin yazar.
/// </summary>
public static class SerilogExtensions
{
    // Saatlik dosya -> 45 gün * 24 saat.
    private const int RetainedFileCount = 45 * 24;

    private const string ConsoleTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Src} {Message:lj}{NewLine}{Exception}";

    public static WebApplicationBuilder AddAppSerilog(this WebApplicationBuilder builder)
    {
        var logRoot = Path.Combine(builder.Environment.ContentRootPath, "logs");

        builder.Host.UseSerilog((context, services, cfg) => cfg
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.With<ShortSourceContextEnricher>()
            .Enrich.With<TraceIdFallbackEnricher>()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.Console(outputTemplate: ConsoleTemplate)
            .WriteTo.File(
                formatter: new RenderedCompactJsonFormatter(),
                path: Path.Combine(logRoot, "app", "app-.log"),
                rollingInterval: RollingInterval.Hour,
                retainedFileCountLimit: RetainedFileCount,
                shared: true)
            .WriteTo.Logger(sub => sub
                .Filter.ByIncludingOnly(logEvent => logEvent.Level >= LogEventLevel.Error)
                .WriteTo.File(
                    formatter: new RenderedCompactJsonFormatter(),
                    path: Path.Combine(logRoot, "errors", "error-.log"),
                    rollingInterval: RollingInterval.Hour,
                    retainedFileCountLimit: RetainedFileCount,
                    shared: true)));

        return builder;
    }
}
