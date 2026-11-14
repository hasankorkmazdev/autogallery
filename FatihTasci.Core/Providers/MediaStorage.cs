using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace FatihTasci.Core.Providers;

/// <summary>
/// Dosya deposu. Kök klasör Storage:Root ayarından okunur (göreli ise uygulama klasörüne göre).
///   {Root}/public  -> sitede /media altında sunulur (WebP varyantları, ekspertiz PDF'leri)
///   {Root}/private -> sunulmaz (orijinal fotoğraflar, talep ekleri)
/// Veritabanında yollar ilgili köke göre göreli ve '/' ayraçlı saklanır.
/// </summary>
public interface IMediaStorage
{
    string PublicRoot { get; }
    string PrivateRoot { get; }

    Task SavePublicAsync(string relativePath, Stream content);
    Task SavePrivateAsync(string relativePath, Stream content);
    void DeletePublic(string relativePath);
    void DeletePrivate(string relativePath);
    string GetFullPublicPath(string relativePath);
    string GetFullPrivatePath(string relativePath);

    /// <summary>
    /// Göreli public yolu site adresine çevirir: vehicles/x.webp -> /media/vehicles/x.webp
    /// </summary>
    string ToUrl(string relativePath);
}

public class LocalMediaStorage : IMediaStorage
{
    public const string RequestPath = "/media";

    public LocalMediaStorage(IConfiguration configuration, IWebHostEnvironment env)
    {
        var root = configuration.GetValue<string>("Storage:Root");
        if (string.IsNullOrWhiteSpace(root))
            root = "Storage";
        if (!Path.IsPathRooted(root))
            root = Path.Combine(env.ContentRootPath, root);

        PublicRoot = Path.Combine(root, "public");
        PrivateRoot = Path.Combine(root, "private");
        Directory.CreateDirectory(PublicRoot);
        Directory.CreateDirectory(PrivateRoot);
    }

    public string PublicRoot { get; }
    public string PrivateRoot { get; }

    public Task SavePublicAsync(string relativePath, Stream content) => SaveAsync(PublicRoot, relativePath, content);

    public Task SavePrivateAsync(string relativePath, Stream content) => SaveAsync(PrivateRoot, relativePath, content);

    public void DeletePublic(string relativePath) => Delete(PublicRoot, relativePath);

    public void DeletePrivate(string relativePath) => Delete(PrivateRoot, relativePath);

    public string GetFullPublicPath(string relativePath) => Resolve(PublicRoot, relativePath);

    public string GetFullPrivatePath(string relativePath) => Resolve(PrivateRoot, relativePath);

    public string ToUrl(string relativePath)
    {
        return string.IsNullOrEmpty(relativePath) ? null : $"{RequestPath}/{relativePath}";
    }

    private static async Task SaveAsync(string root, string relativePath, Stream content)
    {
        var fullPath = Resolve(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        await using var file = File.Create(fullPath);
        await content.CopyToAsync(file);
    }

    private static void Delete(string root, string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return;

        var fullPath = Resolve(root, relativePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    /// <summary>
    /// Göreli yolu köke bağlar; kökün dışına çıkan yolları (..) reddeder.
    /// </summary>
    private static string Resolve(string root, string relativePath)
    {
        var fullRoot = Path.GetFullPath(root);
        var fullPath = Path.GetFullPath(Path.Combine(fullRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(fullRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Geçersiz dosya yolu.");
        return fullPath;
    }
}
