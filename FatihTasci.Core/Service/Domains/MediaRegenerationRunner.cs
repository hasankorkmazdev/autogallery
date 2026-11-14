using FatihTasci.Core.Models.Domain.Setting;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FatihTasci.Core.Service.Domains;

/// <summary>
/// "Filigranı yeniden uygula" işi. İstek beklemeden arka planda çalışır (yüzlerce fotoğraf dakikalar sürebilir,
/// IIS istek zaman aşımına takılmasın). Durum bellekte tutulur; uygulama kapanırsa iş iptal edilir,
/// tekrar başlatılabilir (her fotoğraf orijinalinden üretildiği için yarım kalan iş zarar vermez).
/// </summary>
public class MediaRegenerationRunner : IMediaRegenerationRunner
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<MediaRegenerationRunner> _logger;
    private readonly object _lock = new();
    private WatermarkJobStatusModel _status = new();

    public MediaRegenerationRunner(IServiceScopeFactory scopeFactory, IHostApplicationLifetime lifetime, ILogger<MediaRegenerationRunner> logger)
    {
        _scopeFactory = scopeFactory;
        _lifetime = lifetime;
        _logger = logger;
    }

    public WatermarkJobStatusModel GetStatus()
    {
        lock (_lock)
        {
            return new WatermarkJobStatusModel
            {
                IsRunning = _status.IsRunning,
                Total = _status.Total,
                Processed = _status.Processed,
                Failed = _status.Failed,
                StartedAt = _status.StartedAt,
                FinishedAt = _status.FinishedAt
            };
        }
    }

    public bool TryStart()
    {
        lock (_lock)
        {
            if (_status.IsRunning)
                return false;

            _status = new WatermarkJobStatusModel { IsRunning = true, StartedAt = DateTime.Now };
        }

        _ = Task.Run(RunAsync);
        return true;
    }

    private async Task RunAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var mediaService = scope.ServiceProvider.GetRequiredService<IMediaService>();
            await mediaService.RegenerateAllVariantsAsync(
                total =>
                {
                    lock (_lock) _status.Total = total;
                },
                success =>
                {
                    lock (_lock)
                    {
                        _status.Processed++;
                        if (!success)
                            _status.Failed++;
                    }
                },
                _lifetime.ApplicationStopping);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Filigran işi uygulama kapanırken durduruldu.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Filigran işi hata ile sonlandı.");
        }
        finally
        {
            lock (_lock)
            {
                _status.IsRunning = false;
                _status.FinishedAt = DateTime.Now;
            }
        }
    }
}
