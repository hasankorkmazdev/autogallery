using FatihTasci.Core.Providers;
using Microsoft.Extensions.Logging;

namespace FatihTasci.Data.Context.Seed;

/// <summary>
/// Seed'in yazdığı medya dosyalarını siler. Silinemeyen dosya uyarı olarak loglanır, seed durmaz.
/// </summary>
internal static class SeedFiles
{
    public static void Delete(IMediaStorage storage, ILogger logger, IEnumerable<string> publicPaths, IEnumerable<string> privatePaths)
    {
        foreach (var path in publicPaths.Where(x => !string.IsNullOrEmpty(x)))
            TryDelete(() => storage.DeletePublic(path), path, logger);
        foreach (var path in privatePaths.Where(x => !string.IsNullOrEmpty(x)))
            TryDelete(() => storage.DeletePrivate(path), path, logger);
    }

    private static void TryDelete(Action delete, string path, ILogger logger)
    {
        try
        {
            delete();
        }
        catch (IOException ex)
        {
            logger.LogWarning(ex, "Dosya silinemedi: {Path}", path);
        }
    }
}
