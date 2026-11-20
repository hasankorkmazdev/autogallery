using FatihTasci.Core.Entity.Catalog;
using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FatihTasci.Data.Context.Seed.Vehicle;

/// <summary>
/// Renkler, donanımlar ve katalog (marka / model / paket): yalnız veritabanında olmayanlar eklenir (SeedHelper).
/// Panelde adı, sırası ya da slug'ı değişen kayda dokunulmaz.
/// </summary>
internal static class VehicleSeeder
{
    /// <summary>
    /// Marka logoları Server'ın wwwroot/images/brands klasöründen statik dosya olarak sunulur.
    /// </summary>
    private const string BrandLogoFolder = "/images/brands";

    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedColorsAsync(context);
        await SeedFeaturesAsync(context);
        await SeedCatalogAsync(context);
    }

    /// <summary>
    /// Boş veritabanında koddaki sırayla; mevcut veritabanına sonradan eklenen renk listenin sonuna.
    /// </summary>
    private static async Task SeedColorsAsync(AppDbContext context)
    {
        var last = await context.Colors.IgnoreQueryFilters().MaxAsync(x => (int?)x.SortOrder);
        var colors = VehicleSeedData.Colors.Select((x, i) => new ColorEntity { Id = x.Id, Name = x.Name, HexCode = x.Hex, SortOrder = i });

        await SeedHelper.AddMissingAsync(context, colors, x => x.Name, color =>
        {
            if (last != null)
                color.SortOrder = (last = last + 1).Value;
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Boş kategoride koddaki sırayla; mevcut veritabanına sonradan eklenen donanım kategorisinin sonuna.
    /// </summary>
    private static async Task SeedFeaturesAsync(AppDbContext context)
    {
        var last = await context.Features.IgnoreQueryFilters()
            .GroupBy(x => x.Category)
            .Select(g => new { g.Key, SortOrder = g.Max(x => x.SortOrder) })
            .ToDictionaryAsync(x => x.Key, x => x.SortOrder);
        var features = VehicleSeedData.Features.SelectMany(group => group.Features.Select((x, i) =>
            new FeatureEntity { Id = x.Id, Category = group.Category, Name = x.Name, SortOrder = i }));

        await SeedHelper.AddMissingAsync(context, features, x => $"{(int)x.Category}/{x.Name}", feature =>
        {
            if (last.TryGetValue(feature.Category, out var order))
                feature.SortOrder = last[feature.Category] = order + 1;
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Marka / model / paket; benzersizlik slug'a göre (marka içinde model, model içinde paket). Atlanan (aynı slug'la
    /// panelden eklenmiş) markanın modelleri, atlanan modelin paketleri eklenmez.
    /// </summary>
    private static async Task SeedCatalogAsync(AppDbContext context)
    {
        var brands = CatalogSeedData.Brands.Select((b, i) => new BrandEntity
        {
            Id = b.Id,
            Name = b.Name,
            Slug = b.Slug,
            LogoPath = $"{BrandLogoFolder}/{b.Slug}.png",
            SortOrder = i + 1
        });
        await SeedHelper.AddMissingAsync(context, brands, x => x.Slug);

        var brandIds = (await context.Brands.IgnoreQueryFilters().Select(x => x.Id).ToListAsync()).ToHashSet();
        var series = CatalogSeedData.Brands.Where(b => brandIds.Contains(b.Id)).SelectMany(b => b.Series.Select((s, i) => new SeriesEntity
        {
            Id = s.Id,
            BrandId = b.Id,
            Name = s.Name,
            Slug = SlugHelper.Generate(s.Name),
            SortOrder = i + 1
        }));
        await SeedHelper.AddMissingAsync(context, series, x => $"{x.BrandId}/{x.Slug}");

        var seriesIds = (await context.Series.IgnoreQueryFilters().Select(x => x.Id).ToListAsync()).ToHashSet();
        var models = CatalogSeedData.Brands.SelectMany(b => b.Series).Where(s => seriesIds.Contains(s.Id)).SelectMany(s => s.Models.Select(m =>
            new CarModelEntity
            {
                Id = m.Id,
                SeriesId = s.Id,
                Name = m.Name,
                Slug = SlugHelper.Generate(m.Name)
            }));
        await SeedHelper.AddMissingAsync(context, models, x => $"{x.SeriesId}/{x.Slug}");
    }
}
