using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace FatihTasci.Core.Logging;

/// <summary>
/// SourceContext genelde tam namespace olur (ör. Microsoft.EntityFrameworkCore.Database.Command).
/// Bunu son parcaya indirger (Command) ve "Src" property'si olarak ekler. Yoksa "-" yazar.
/// </summary>
public sealed class ShortSourceContextEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var shortName = "-";
        if (logEvent.Properties.TryGetValue("SourceContext", out var value)
            && value is ScalarValue { Value: string source } && source.Length > 0)
        {
            var idx = source.LastIndexOf('.');
            shortName = idx >= 0 && idx < source.Length - 1 ? source[(idx + 1)..] : source;
        }

        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Src", shortName));
    }
}

/// <summary>
/// Serilog.Extensions.Logging aktif <see cref="Activity"/> varken loglara otomatik olarak
/// "TraceId" ekler; bu enricher yalnizca eksik oldugu durumlarda (Activity yoksa, or. uygulama
/// acilisi) "-" ile doldurur ki sablondaki {TraceId} her zaman cozulsun. Ayni istekte uretilen
/// device / app / hata satirlari boylece ayni TraceId ile eslesir (cross-cutting izleme).
/// </summary>
public sealed class TraceIdFallbackEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.ContainsKey("TraceId"))
            return;

        var traceId = Activity.Current?.TraceId.ToString();
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
            "TraceId", string.IsNullOrEmpty(traceId) ? "-" : traceId));
    }
}
