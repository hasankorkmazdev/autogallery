using FatihTasci.Core.Entity.Common;
using FatihTasci.Core.Models.Domain.Setting;
using FatihTasci.Core.Providers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FatihTasci.Data.Context.Seed.Site;

/// <summary>
/// Site ayarları (anahtara göre), showroom sahneleri (Id ya da başlığa göre) ve yerelden dışa aktarılan site verisi:
/// ayarlar (logo, içerik yazıları...) ve showroom fotoğrafları (scripts/export-site-seed.ps1 → Server/Assets/Seed/Site:
/// site.json + Storage'daki göreli yollarıyla dosyalar). Yalnız eksik olan eklenir: var olan ayara (boş değer dahil) ve
/// sahneye dokunulmaz; site.json'daki fotoğraf yalnız yeni eklenen sahneye konur. site.json yayın paketine yalnız
/// -p:IncludeDemoData=true ile girer.
/// </summary>
internal static class SiteSeeder
{
    public static async Task SeedAsync(AppDbContext context, IMediaStorage storage, string siteFolder, ILogger logger)
    {
        var file = storage == null || string.IsNullOrEmpty(siteFolder) ? null : Path.Combine(siteFolder, "site.json");
        using var siteData = File.Exists(file) ? JsonDocument.Parse(await File.ReadAllTextAsync(file)) : null;
        var root = siteData?.RootElement;

        await AddSettingsAsync(context, storage, siteFolder, root, logger);
        await AddScenesAsync(context, storage, siteFolder, root, logger);
    }

    /// <summary>
    /// Önce site.json'daki ayarlar, sonra koddaki varsayılanlar: ikisinde de olan anahtar site.json'daki değerle eklenir.
    /// </summary>
    private static async Task AddSettingsAsync(AppDbContext context, IMediaStorage storage, string siteFolder, JsonElement? root,
        ILogger logger)
    {
        var keys = (await context.SiteSettings.IgnoreQueryFilters().Select(x => x.Key).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        void Add(string key, string value)
        {
            context.SiteSettings.Add(new SiteSettingEntity { Key = key, Value = value });
            keys.Add(key);
        }

        if (root != null)
        {
            foreach (var item in root.Value.GetProperty("Settings").EnumerateArray())
            {
                var key = item.GetProperty("Key").GetString();
                var value = item.GetProperty("Value").GetString();
                // "__" önekli anahtarlar eski seed işaretleridir, alınmaz.
                if (string.IsNullOrEmpty(key) || key.StartsWith("__", StringComparison.Ordinal) || string.IsNullOrEmpty(value) || keys.Contains(key))
                    continue;
                if (key == nameof(SiteSettingsModel.LogoPath) && !await CopyFileAsync(storage, siteFolder, isPublic: true, value, logger))
                    continue;
                Add(key, value);
            }
        }

        foreach (var (key, value) in SiteSeedData.Settings.Where(x => !keys.Contains(x.Key)))
            Add(key, value);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Eksik sahneleri ekler; site.json'da aynı başlıklı sahnenin fotoğrafı varsa dosyalarıyla birlikte ona konur.
    /// </summary>
    private static async Task AddScenesAsync(AppDbContext context, IMediaStorage storage, string siteFolder, JsonElement? root,
        ILogger logger)
    {
        var photos = new Dictionary<string, JsonElement>(SeedHelper.NameComparer);
        if (root != null)
        {
            foreach (var item in root.Value.GetProperty("Showroom").EnumerateArray())
                photos.TryAdd(item.GetProperty("Title").GetString() ?? string.Empty, item);
        }

        var scenes = SiteSeedData.ShowroomScenes.Select((x, i) => new ShowroomSceneEntity { Id = x.Id, Tag = x.Tag, Title = x.Title, SortOrder = i });
        await SeedHelper.AddMissingAsync(context, scenes, x => x.Title, async scene =>
        {
            if (!photos.TryGetValue(scene.Title, out var item))
                return;

            var original = item.GetProperty("OriginalPath").GetString();
            var medium = item.GetProperty("MediumPath").GetString();
            var large = item.GetProperty("LargePath").GetString();
            if (!await CopyFileAsync(storage, siteFolder, isPublic: false, original, logger)
                || !await CopyFileAsync(storage, siteFolder, isPublic: true, medium, logger)
                || !await CopyFileAsync(storage, siteFolder, isPublic: true, large, logger))
                return;

            scene.OriginalPath = original;
            scene.MediumPath = medium;
            scene.LargePath = large;
        });
    }

    /// <summary>
    /// Site klasöründeki dosyayı (public/ ya da private/ altında, Storage'daki göreli yoluyla) Storage'da aynı yola
    /// kopyalar; Storage'da zaten varsa dokunmaz.
    /// </summary>
    private static async Task<bool> CopyFileAsync(IMediaStorage storage, string siteFolder, bool isPublic, string relativePath,
        ILogger logger)
    {
        if (string.IsNullOrEmpty(relativePath))
            return false;

        var relative = relativePath.Replace('/', Path.DirectorySeparatorChar);
        if (File.Exists(Path.Combine(isPublic ? storage.PublicRoot : storage.PrivateRoot, relative)))
            return true;

        var source = Path.Combine(siteFolder, isPublic ? "public" : "private", relative);
        if (!File.Exists(source))
        {
            logger.LogWarning("Site verisi dosyası bulunamadı: {Path}", source);
            return false;
        }

        await using var stream = File.OpenRead(source);
        if (isPublic)
            await storage.SavePublicAsync(relativePath, stream);
        else
            await storage.SavePrivateAsync(relativePath, stream);
        return true;
    }
}
