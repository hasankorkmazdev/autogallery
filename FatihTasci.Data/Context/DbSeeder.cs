using FatihTasci.Core.Providers;
using FatihTasci.Data.Context.Seed.Account;
using FatihTasci.Data.Context.Seed.Delivery;
using FatihTasci.Data.Context.Seed.Listing;
using FatihTasci.Data.Context.Seed.Site;
using FatihTasci.Data.Context.Seed.Vehicle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FatihTasci.Data.Context;

/// <summary>
/// Başlangıç verisi, yalnız ekler: koddaki kayıt veritabanında yoksa (Id'si de adı / slug'ı / anahtarı da) aynı Id ile
/// eklenir, varsa dokunulmaz. Panelde değiştirilen ya da kullanıcıların eklediği kayıtlar olduğu gibi kalır; seed hiçbir
/// şeyi güncellemez ve silmez (koddaki bir kayıt sonradan değişirse mevcut veritabanlarına yansımaz). Yalnız
/// <c>Seed:Enabled</c> açıkken çalışır. Seed'ler alanlarına göre Seed klasöründedir (Account, Vehicle, Site, Listing, Delivery);
/// verileri yanlarındaki *SeedData'dadır.
/// </summary>
public static class DbSeeder
{
    /// <param name="seedFolder">
    /// Server/Assets/Seed: Site (dışa aktarılan site verisi), Vehicles (ilan fotoğrafları) ve Deliveries (örnek teslimat
    /// fotoğrafları) alt klasörleri.
    /// </param>
    /// <param name="isDevelopment">Geliştirme ortamında panelin test kullanıcısı da eklenir.</param>
    public static async Task SeedAsync(AppDbContext context, IConfiguration configuration, IMediaStorage storage, string seedFolder,
        bool isDevelopment, ILogger logger)
    {
        if (!IsEnabled(configuration, "Seed:Enabled"))
            return;

        await AccountSeeder.SeedAsync(context, isDevelopment);
        await VehicleSeeder.SeedAsync(context);
        await SiteSeeder.SeedAsync(context, storage, string.IsNullOrEmpty(seedFolder) ? null : Path.Combine(seedFolder, "Site"), logger);

        // Katalog ve logodan sonra: ilan paketi katalogda olmalı, fotoğraflara filigran ayara göre basılır.
        if (!string.IsNullOrEmpty(seedFolder))
            await ListingSeeder.SeedAsync(context, storage, Path.Combine(seedFolder, "Vehicles"), logger);

        // Örnek teslimatlar yalnız DemoData açıkken eklenir; kapatılınca silinmez.
        if (IsEnabled(configuration, "DemoData:Enabled") && !string.IsNullOrEmpty(seedFolder))
            await DemoDeliverySeeder.SeedAsync(context, storage, Path.Combine(seedFolder, "Deliveries"), logger);
    }

    private static bool IsEnabled(IConfiguration configuration, string key)
    {
        return bool.TryParse(configuration[key], out var value) && value;
    }
}
