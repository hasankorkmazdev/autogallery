using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;

namespace FatihTasci.Server.Helpers;

/// <summary>
/// Yalnız geliştirmede: DevTools:StartViteDevServer açıksa sunucuyla birlikte panelin Vite geliştirme sunucusunu
/// (FatihTasci.Admin, npm run dev → http://localhost:5173/panel/) başlatır ve sunucu kapanınca kapatır.
/// Visual Studio'daki "Panel" profili bunu açar; böylece F5 ile hem API hem panel ayağa kalkar.
/// npm run dev öncesi "predev" 5173'ü tutan eski Vite'ı kapatır. Uygulama "başladı" sayılmadan önce Vite'ın
/// portu açması beklenir, böylece tarayıcı hazır olmayan bir adrese açılmaz.
/// </summary>
public class ViteDevServer : IHostedService, IDisposable
{
    public const int Port = 5173;
    private static readonly TimeSpan StartTimeout = TimeSpan.FromSeconds(60);

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ViteDevServer> _logger;
    private Process _process;

    public ViteDevServer(IWebHostEnvironment env, ILogger<ViteDevServer> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var adminDir = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..", "FatihTasci.Admin"));
        if (!File.Exists(Path.Combine(adminDir, "package.json")))
        {
            _logger.LogWarning("Panel klasörü bulunamadı, Vite başlatılmadı: {Path}", adminDir);
            return;
        }

        // Eski Vite hâlâ dinliyorsa önce kapatılır; yoksa hazır kontrolü yeni yerine eskisini görür.
        if (await IsListeningAsync())
            await FreePortAsync(cancellationToken);

        var startInfo = OperatingSystem.IsWindows()
            ? new ProcessStartInfo("cmd.exe", "/c npm run dev")
            : new ProcessStartInfo("npm", "run dev");
        startInfo.WorkingDirectory = adminDir;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        _logger.LogInformation("Panel geliştirme sunucusu (Vite) başlatılıyor: {Path}", adminDir);
        _process = Process.Start(startInfo);
        if (_process == null)
        {
            _logger.LogWarning("Vite başlatılamadı.");
            return;
        }

        _process.OutputDataReceived += (_, e) => Log(e.Data, LogLevel.Debug);
        _process.ErrorDataReceived += (_, e) => Log(e.Data, LogLevel.Warning);
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        var deadline = DateTime.UtcNow + StartTimeout;
        while (DateTime.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
        {
            if (_process.HasExited)
            {
                _logger.LogWarning("Vite kapandı (çıkış kodu {Code}). Panel http://localhost:{Port}/panel/ adresinde açılmayacak.", _process.ExitCode, Port);
                return;
            }
            if (await IsListeningAsync())
            {
                _logger.LogInformation("Panel hazır: http://localhost:{Port}/panel/", Port);
                return;
            }
            await Task.Delay(250, cancellationToken);
        }

        _logger.LogWarning("Vite {Seconds} sn içinde açılmadı; arka planda başlamaya devam ediyor.", StartTimeout.TotalSeconds);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        StopProcess();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        StopProcess();
        _process?.Dispose();
    }

    private void StopProcess()
    {
        try
        {
            if (_process is { HasExited: false })
            {
                _logger.LogInformation("Panel geliştirme sunucusu (Vite) kapatılıyor.");
                _process.Kill(entireProcessTree: true);
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or Win32Exception)
        {
            _logger.LogDebug(ex, "Vite zaten kapanmış.");
        }
    }

    /// <summary>
    /// scripts/free-port.ps1 ile 5173'ü tutan süreci kapatır (Windows). Diğer sistemlerde predev'e bırakılır.
    /// </summary>
    private async Task FreePortAsync(CancellationToken cancellationToken)
    {
        var script = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..", "scripts", "free-port.ps1"));
        if (!OperatingSystem.IsWindows() || !File.Exists(script))
            return;

        var startInfo = new ProcessStartInfo("powershell", $"-NoProfile -ExecutionPolicy Bypass -File \"{script}\" -Port {Port}")
        {
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = Process.Start(startInfo);
        if (process != null)
            await process.WaitForExitAsync(cancellationToken).WaitAsync(TimeSpan.FromSeconds(20), cancellationToken);
        _logger.LogInformation("5173'ü tutan eski Vite kapatıldı.");
    }

    private void Log(string line, LogLevel level)
    {
        if (!string.IsNullOrWhiteSpace(line))
            _logger.Log(level, "[vite] {Line}", line);
    }

    private static async Task<bool> IsListeningAsync()
    {
        foreach (var host in new[] { "localhost", "127.0.0.1", "::1" })
        {
            try
            {
                using var client = new TcpClient();
                using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
                await client.ConnectAsync(host, Port, timeout.Token);
                return true;
            }
            catch (Exception ex) when (ex is SocketException or OperationCanceledException)
            {
            }
        }
        return false;
    }
}
