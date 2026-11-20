using FatihTasci.Core.Entity.Crm;
using FatihTasci.Core.Providers;
using FatihTasci.Core.Service.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FatihTasci.Data.Context.Seed.Delivery;

/// <summary>
/// Örnek teslimatlar (DemoDeliverySeedData), yalnız DemoData açıkken: Id'si veritabanında olmayan eklenir (fotoğraf panel
/// yüklemesiyle aynı yoldan işlenir, teslim tarihi eklendiği güne göre), var olana dokunulmaz. Panelde silinen geri gelmez;
/// DemoData kapatılınca da silinmez, gerekirse panelden silinir. Canlıda stok fotoğraf ve uydurma müşteri adı gerçek
/// teslimat gibi görünür.
/// </summary>
internal static class DemoDeliverySeeder
{
    public static async Task SeedAsync(AppDbContext context, IMediaStorage storage, string folder, ILogger logger)
    {
        if (storage == null || !Directory.Exists(folder))
            return;

        var ids = DemoDeliverySeedData.Samples.Select(x => x.Id).ToList();
        var existing = (await context.Deliveries.IgnoreQueryFilters().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync()).ToHashSet();
        var written = new List<(bool IsPublic, string Path)>();
        var added = 0;

        try
        {
            foreach (var sample in DemoDeliverySeedData.Samples.Where(x => !existing.Contains(x.Id)))
            {
                var path = Path.Combine(folder, sample.File);
                if (!File.Exists(path))
                {
                    logger.LogWarning("Örnek teslimat fotoğrafı bulunamadı: {Path}", path);
                    continue;
                }

                var delivery = new DeliveryEntity
                {
                    Id = sample.Id,
                    VehicleTitle = sample.Vehicle,
                    CustomerName = sample.Customer,
                    City = sample.City,
                    DeliveredAt = DateTime.Today.AddDays(-sample.DaysAgo)
                };

                await using var stream = File.OpenRead(path);
                var files = await DeliveryService.WriteImagesAsync(storage, delivery, stream, Path.GetExtension(path), logger);
                if (files == null)
                {
                    logger.LogWarning("Örnek teslimat fotoğrafı okunamadı: {Path}", path);
                    continue;
                }

                written.AddRange(files);
                context.Deliveries.Add(delivery);
                added++;
            }

            await SeedHelper.SaveWithIdsAsync<DeliveryEntity>(context);
        }
        catch
        {
            // Kaydedilemeyen teslimatların dosyaları silinir; sonraki açılışta yeniden eklenir.
            SeedFiles.Delete(storage, logger,
                written.Where(x => x.IsPublic).Select(x => x.Path),
                written.Where(x => !x.IsPublic).Select(x => x.Path));
            throw;
        }

        if (added > 0)
            logger.LogInformation("{Count} örnek teslimat eklendi.", added);
    }
}
