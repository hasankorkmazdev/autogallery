using FatihTasci.Core.Entity.Catalog;
using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Enums;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain.Setting;
using FatihTasci.Core.Providers;
using FatihTasci.Core.Service.Domains;
using FatihTasci.Data.Context.Seed.Account;
using FatihTasci.Data.Context.Seed.Vehicle;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace FatihTasci.Data.Context.Seed.Listing;

/// <summary>
/// Galerinin ilanları (ListingSeedData): Id'si veritabanında olmayan ilan yayında olarak eklenir, var olana dokunulmaz.
/// Panelde silinen (IsDelete) geri gelmez, panelde değişen (fiyat, durum, fotoğraf) olduğu gibi kalır. Fotoğraflar
/// Server/Assets/Seed/Vehicles/{klasör}'den panel yüklemesiyle aynı yoldan işlenir (filigran ayara göre). Klasör yoksa
/// (yayın paketi IncludeDemoData'sız) hiçbir şey yapılmaz; sonraki açılışta eklenir.
/// </summary>
internal static class ListingSeeder
{
    private static readonly string[] PhotoExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public static async Task SeedAsync(AppDbContext context, IMediaStorage storage, string folder, ILogger logger)
    {
        if (storage == null || !Directory.Exists(folder))
            return;

        var listings = ListingSeedData.Create();
        var ids = listings.Select(x => x.Vehicle.Id).ToList();
        var existing = (await context.Vehicles.IgnoreQueryFilters().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync()).ToHashSet();
        var pending = listings.Where(x => !existing.Contains(x.Vehicle.Id)).ToList();
        if (pending.Count == 0)
            return;

        // Danışmanı verilmeyen ilanın danışmanı ListingSeedData.AdvisorId; o hesap yoksa ya da pasifse Yönetici (AccountSeedData),
        // o da yoksa boş kalır.
        var advisorCandidates = new[] { ListingSeedData.AdvisorId, AccountSeedData.Users[0].Id };
        var activeAdvisors = await context.Users.Where(x => advisorCandidates.Contains(x.Id) && x.IsActive).Select(x => x.Id).ToListAsync();
        var defaultAdvisorId = advisorCandidates.FirstOrDefault(activeAdvisors.Contains);

        var colorIds = (await context.Colors.Select(x => x.Id).ToListAsync()).ToHashSet();
        var featureIds = (await context.Features.Select(x => x.Id).ToListAsync()).ToHashSet();
        var writtenPublic = new List<string>();
        var writtenPrivate = new List<string>();
        var added = 0;

        using var watermark = await LoadWatermarkAsync(context, storage, logger);

        try
        {
            foreach (var listing in pending)
            {
                var photos = FindPhotos(Path.Combine(folder, listing.Folder));
                if (photos.Count == 0)
                {
                    logger.LogWarning("İlan fotoğrafı bulunamadı: {Folder}", listing.Folder);
                    continue;
                }

                var model = await FindModelAsync(context, listing.Brand, listing.Series, listing.Model);
                if (model == null)
                {
                    logger.LogWarning("İlanın kataloğu bulunamadı: {Brand} / {Series} / {Model}", listing.Brand, listing.Series, listing.Model);
                    continue;
                }

                var vehicle = listing.Vehicle;
                vehicle.BrandId = model.Series.BrandId;
                vehicle.SeriesId = model.SeriesId;
                vehicle.CarModelId = model.Id;
                vehicle.Slug = SlugHelper.ForVehicle(vehicle.Year, model.Series.Brand.Name, model.Name);
                vehicle.Description = VehicleService.SanitizeDescription(vehicle.Description);
                vehicle.Status = VehicleStatus.Published;
                vehicle.PublishedAt ??= DateTime.Now;
                if (vehicle.AdvisorId == null && defaultAdvisorId != Guid.Empty)
                    vehicle.AdvisorId = defaultAdvisorId;

                var colorId = VehicleSeedData.Colors.FirstOrDefault(x => x.Name == listing.Color).Id;
                vehicle.ExteriorColorId = colorIds.Contains(colorId) ? colorId : null;

                foreach (var featureId in FeatureIds(listing.Features, logger).Where(featureIds.Contains))
                    vehicle.Features.Add(new VehicleFeatureEntity { FeatureId = featureId });

                foreach (var photo in photos)
                {
                    await using var stream = File.OpenRead(photo);
                    var media = await MediaService.WriteImageAsync(storage, vehicle.Id, stream, Path.GetExtension(photo), watermark,
                        writtenPublic, writtenPrivate);
                    if (media == null)
                    {
                        logger.LogWarning("İlan fotoğrafı okunamadı: {Path}", photo);
                        continue;
                    }

                    media.SortOrder = vehicle.Media.Count;
                    media.IsCover = vehicle.Media.Count == 0;
                    vehicle.Media.Add(media);
                }

                if (vehicle.Media.Count == 0)
                    continue;

                // İlanlar tek tek kaydedilir: ilan numarası (sequence) listedeki sırayı izler.
                context.Vehicles.Add(vehicle);
                await context.SaveChangesAsync();
                writtenPublic.Clear();
                writtenPrivate.Clear();
                added++;
            }
        }
        catch
        {
            // Kaydedilemeyen ilanın dosyaları silinir (önceki ilanlar kaydedildi); sonraki açılışta yeniden eklenir.
            SeedFiles.Delete(storage, logger, writtenPublic, writtenPrivate);
            throw;
        }

        if (added > 0)
            logger.LogInformation("{Count} ilan eklendi.", added);
    }

    private static List<string> FindPhotos(string folder)
    {
        if (!Directory.Exists(folder))
            return [];

        return Directory.GetFiles(folder)
            .Where(x => PhotoExtensions.Contains(Path.GetExtension(x).ToLowerInvariant()))
            .OrderBy(x => Path.GetFileName(x), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Paket katalog seed'indeki Id'siyle; o Id yoksa (aynı adlı paket panelden önce eklenmişse) model içinde slug'ıyla.
    /// </summary>
    private static async Task<CarModelEntity> FindModelAsync(AppDbContext context, string brandName, string seriesName, string modelName)
    {
        var names = SeedHelper.NameComparer;
        var match = CatalogSeedData.Brands
            .Where(b => names.Equals(b.Name, brandName))
            .SelectMany(b => b.Series.Where(s => names.Equals(s.Name, seriesName)))
            .SelectMany(s => s.Models.Where(m => names.Equals(m.Name, modelName)).Select(m => (SeriesId: s.Id, ModelId: m.Id, m.Name)))
            .ToList();
        if (match.Count == 0)
            return null;

        var (seriesId, modelId, name) = match[0];
        var slug = SlugHelper.Generate(name);
        return await context.CarModels
            .Include(x => x.Series).ThenInclude(x => x.Brand)
            .Where(x => x.Id == modelId || (x.SeriesId == seriesId && x.Slug == slug))
            .OrderBy(x => x.Id == modelId ? 0 : 1)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Donanım adları koddaki listeden (VehicleSeedData) Id'ye çevrilir: panelde adı değişen donanım da bulunur.
    /// </summary>
    private static IEnumerable<int> FeatureIds(string[] names, ILogger logger)
    {
        var all = VehicleSeedData.Features.SelectMany(x => x.Features).ToList();
        foreach (var name in names)
        {
            var feature = all.FirstOrDefault(x => x.Name == name);
            if (feature.Name == null)
                logger.LogWarning("İlan donanımı donanım listesinde yok: {Name}", name);
            else
                yield return feature.Id;
        }
    }

    /// <summary>
    /// Panel yüklemesindeki gibi: filigran açık ve logo varsa logo (MediaService.LoadWatermarkAsync), aksi halde null.
    /// </summary>
    private static async Task<SKBitmap> LoadWatermarkAsync(AppDbContext context, IMediaStorage storage, ILogger logger)
    {
        var keys = new[] { nameof(SiteSettingsModel.WatermarkEnabled), nameof(SiteSettingsModel.LogoPath) };
        var settings = await context.SiteSettings.Where(x => keys.Contains(x.Key)).ToDictionaryAsync(x => x.Key, x => x.Value);

        var enabled = settings.TryGetValue(nameof(SiteSettingsModel.WatermarkEnabled), out var value) && bool.TryParse(value, out var flag)
            ? flag
            : new SiteSettingsModel().WatermarkEnabled;
        if (!enabled || !settings.TryGetValue(nameof(SiteSettingsModel.LogoPath), out var logoPath) || string.IsNullOrEmpty(logoPath))
            return null;

        var fullPath = storage.GetFullPublicPath(logoPath);
        if (!File.Exists(fullPath))
            return null;

        var logo = ImageTools.DecodeFile(fullPath);
        if (logo == null)
            logger.LogWarning("Filigran logosu okunamadı: {Path}", logoPath);
        return logo;
    }
}
