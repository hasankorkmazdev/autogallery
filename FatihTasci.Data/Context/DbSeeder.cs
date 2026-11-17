using FatihTasci.Core.Entity.Catalog;
using FatihTasci.Core.Entity.Common;
using FatihTasci.Core.Entity.Crm;
using FatihTasci.Core.Entity.Identity;
using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Enums;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain.Setting;
using FatihTasci.Core.Providers;
using FatihTasci.Core.Service.Domains;
using FatihTasci.Data.Context.Seed;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FatihTasci.Data.Context;

/// <summary>
/// Başlangıç verisi; her açılışta çalışır ve kayıt bazında "yoksa ekle, varsa dokunma" ilkesiyle ilerler:
/// - Renk, donanım, showroom sahnesi: eklenen kayıtların adları SiteSettings'te işaretlenir (SeedListAsync). İşaretli
///   kayıt bir daha eklenmez (panelde silinse ya da adı değişse de); listeye sonradan eklenen kayıt mevcut
///   veritabanına da bir kez eklenir; aynı adla zaten kayıt varsa dokunulmaz.
/// - Ayarlar: eksik varsayılan anahtar eklenir, var olana (boş değer dahil) dokunulmaz.
/// - Katalog: sürüm artınca eksik marka / model / paket slug'a göre eklenir; panelde adı değişen (SlugRedirect) ya da
///   silinen kayıt yeniden eklenmez, hiçbir şey silinmez.
/// - Yönetici: hiç kullanıcı yoksa SeedAdmin ayarından bir kez oluşturulur (bilgiler koda yazılmaz).
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Marka logoları Server'ın wwwroot/images/brands klasöründen statik dosya olarak sunulur.
    /// </summary>
    public const string BrandLogoFolder = "/images/brands";

    /// <summary>
    /// SiteSettings'te uygulanan katalog seed sürümü. Katalog verisi değişince sürüm artırılır.
    /// </summary>
    private const string CatalogSeedVersionKey = "__CatalogSeedVersion";
    private const int CatalogSeedVersion = 4;

    /// <summary>
    /// Örnek teslimatların uygulanan sürümü (SiteSettings). Her sürüm bir kez eklenir, silinen örnek geri gelmez.
    /// Fotoğraflar ya da örnek listesi değişince sürüm artırılır; önceki sürümün örnekleri dosyalarıyla silinir.
    /// </summary>
    private const string DemoDeliveriesSeededKey = "__DemoDeliveriesSeeded";
    private const int DemoDeliveriesSeedVersion = 3;

    /// <summary>
    /// Eklenmiş örnek araçların klasör adları (SiteSettings, virgülle ayrılmış). Her örnek bir kez eklenir; silinen
    /// örnek geri gelmez, listeye yeni eklenen örnek bir sonraki açılışta eklenir.
    /// </summary>
    private const string DemoVehiclesSeededKey = "__DemoVehiclesSeeded";

    /// <summary>
    /// Kayıt bazında seed işaretleri (SiteSettings): eklenmiş ya da zaten var bulunmuş kayıtların adları, satır satır.
    /// </summary>
    private const string SeededColorsKey = "__SeededColors";
    private const string SeededFeaturesKey = "__SeededFeatures";
    private const string SeededShowroomScenesKey = "__SeededShowroomScenes";

    /// <summary>
    /// SiteSettings.Value sütununun uzunluğu (AppDbContext); işaret bundan uzun olamaz.
    /// </summary>
    private const int SettingValueMaxLength = 4000;

    private static readonly StringComparer KeyComparer = StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), true);

    private static readonly string[] DemoVehiclePhotoExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private static readonly StringComparer NameOrder =
        StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), CompareOptions.IgnoreCase | CompareOptions.NumericOrdering);

    /// <param name="demoSeedFolder">
    /// Örnek teslimat (Deliveries) ve örnek araç (Vehicles) fotoğraflarının kök klasörü; yalnız geliştirme ortamında
    /// verilir, null ise örnek veri eklenmez.
    /// </param>
    public static async Task SeedAsync(AppDbContext context, IConfiguration configuration, IPasswordHasher<UserEntity> passwordHasher,
        IMediaStorage storage, string demoSeedFolder, ILogger logger)
    {
        await SeedAdminAsync(context, configuration, passwordHasher, logger);
        await SeedColorsAsync(context);
        await SeedFeaturesAsync(context);
        await SeedCatalogAsync(context);
        await SeedSettingsAsync(context);
        await SeedShowroomScenesAsync(context);
        if (!string.IsNullOrEmpty(demoSeedFolder))
        {
            await SeedDemoDeliveriesAsync(context, storage, Path.Combine(demoSeedFolder, "Deliveries"), logger);
            await SeedDemoVehiclesAsync(context, storage, Path.Combine(demoSeedFolder, "Vehicles"), logger);
        }
    }

    private static async Task SeedAdminAsync(AppDbContext context, IConfiguration configuration, IPasswordHasher<UserEntity> passwordHasher, ILogger logger)
    {
        if (await context.Users.IgnoreQueryFilters().AnyAsync())
            return;

        var section = configuration.GetSection("SeedAdmin");
        var email = section["Email"];
        var password = section["Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("SeedAdmin ayarı boş olduğu için yönetici kullanıcı oluşturulmadı.");
            return;
        }

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = section["Name"] ?? "Galeri",
            Surname = section["Surname"] ?? "Yöneticisi",
            EMail = email.Trim(),
            NormalizedEMail = email.Trim().ToLowerInvariant(),
            Role = UserRole.Admin
        };
        user.PasswordHash = passwordHasher.HashPassword(user, password);

        context.Users.Add(user);
        await context.SaveChangesAsync();
        logger.LogInformation("Yönetici kullanıcı oluşturuldu: {Email}", user.EMail);
    }

    private static async Task SeedColorsAsync(AppDbContext context)
    {
        var colors = new (string Name, string Hex)[]
        {
            ("Beyaz", "#F5F5F5"), ("Siyah", "#111111"), ("Gri", "#808080"), ("Gümüş Gri", "#B8B8B8"),
            ("Füme", "#4A4D52"), ("Lacivert", "#1B2A4A"), ("Mavi", "#2F5DA8"), ("Kırmızı", "#B3202A"),
            ("Bordo", "#6B1E2B"), ("Kahverengi", "#5C4033"), ("Bej", "#D8C8A8"), ("Şampanya", "#C8A96A"),
            ("Yeşil", "#2F5D3A"), ("Turuncu", "#D2691E"), ("Sarı", "#E0B000"), ("Mor", "#5B3A7A")
        };

        var existing = await context.Colors.Select(x => new { x.Name, x.SortOrder }).ToListAsync();
        var nextOrder = existing.Count == 0 ? 0 : existing.Max(x => x.SortOrder) + 1;
        await SeedListAsync(context, SeededColorsKey, colors.Select(x => x.Name), existing.Select(x => x.Name), name =>
        {
            var color = colors.First(x => x.Name == name);
            context.Colors.Add(new ColorEntity { Name = color.Name, HexCode = color.Hex, SortOrder = nextOrder++ });
        });
    }

    private static async Task SeedFeaturesAsync(AppDbContext context)
    {
        var groups = new Dictionary<FeatureCategory, string[]>
        {
            [FeatureCategory.Safety] = new[]
            {
                "ABS", "ESP", "ASR (Çekiş Kontrolü)", "Sürücü Hava Yastığı", "Yolcu Hava Yastığı", "Yan Hava Yastıkları",
                "Perde Hava Yastıkları", "Diz Hava Yastığı", "Isofix", "Kör Nokta Uyarısı", "Şerit Takip", "Şeritte Tutma Asistanı",
                "Ön Çarpışma Uyarısı", "Acil Fren Asistanı", "Yorgunluk Tespiti", "Yokuş Kalkış Desteği", "Lastik Basınç İzleme",
                "Gece Görüş", "Merkezi Kilit", "Immobilizer", "Alarm", "Çocuk Kilidi"
            },
            [FeatureCategory.Interior] = new[]
            {
                "Deri Koltuk", "Kumaş Koltuk", "Alcantara", "Elektrikli Ön Koltuklar", "Hafızalı Koltuklar", "Isıtmalı Ön Koltuklar",
                "Isıtmalı Arka Koltuklar", "Soğutmalı Koltuklar", "Masajlı Koltuklar", "Manuel Klima", "Otomatik Klima",
                "Çift Bölgeli Klima", "Hız Sabitleyici", "Adaptif Hız Sabitleyici", "Head-up Display", "Dijital Gösterge Paneli",
                "Isıtmalı Direksiyon", "Fonksiyonel Direksiyon", "Anahtarsız Giriş ve Çalıştırma", "Start/Stop",
                "Elektrikli Ön Camlar", "Elektrikli Arka Camlar", "Ambiyans Aydınlatma", "Elektrikli Bagaj Kapağı",
                "Yol Bilgisayarı", "Geri Görüş Kamerası", "360° Kamera", "Otomatik Park Asistanı"
            },
            [FeatureCategory.Exterior] = new[]
            {
                "Sunroof", "Panoramik Cam Tavan", "LED Far", "Adaptif / Matrix LED Far", "Xenon Far", "Far Yıkama", "Sis Farı",
                "Far Sensörü", "Yağmur Sensörü", "Ön Park Sensörü", "Arka Park Sensörü", "Elektrikli Aynalar", "Isıtmalı Aynalar",
                "Elektrikli Katlanır Aynalar", "Otomatik Kararan Ayna", "Alaşım Jant", "Römork Demiri", "Havalı Süspansiyon",
                "Spoiler", "Karartılmış Arka Camlar"
            },
            [FeatureCategory.Multimedia] = new[]
            {
                "Bluetooth", "USB / AUX", "Apple CarPlay", "Android Auto", "Navigasyon", "Kablosuz Şarj",
                "Premium Ses Sistemi", "Dokunmatik Ekran", "Sesli Komut", "Arka Eğlence Sistemi"
            }
        };

        // Yeni donanım kategorisinin sonuna eklenir (kategori içi sıra panelde değişebilir)
        var items = groups.SelectMany(g => g.Value.Select(name => (Category: g.Key, Name: name))).ToList();
        var existing = await context.Features.Select(x => new { x.Name, x.Category, x.SortOrder }).ToListAsync();
        var nextOrder = existing.GroupBy(x => x.Category).ToDictionary(g => g.Key, g => g.Max(x => x.SortOrder) + 1);
        await SeedListAsync(context, SeededFeaturesKey, items.Select(x => x.Name), existing.Select(x => x.Name), name =>
        {
            var item = items.First(x => x.Name == name);
            var order = nextOrder.TryGetValue(item.Category, out var value) ? value : 0;
            nextOrder[item.Category] = order + 1;
            context.Features.Add(new FeatureEntity { Category = item.Category, Name = item.Name, SortOrder = order });
        });
    }

    /// <summary>
    /// Katalog: SpareParts'taki 104 marka (logolarıyla) ve serileri, üstüne motor + paket seviyesinde
    /// örnek modeller. Var olan veritabanıyla birleşir (eksik olan eklenir, hiçbir şey silinmez).
    /// Sürüm anahtarı sayesinde yalnız bir kez çalışır; panelde yapılan değişiklikler sonraki açılışlarda ezilmez.
    /// Eksik model araç girerken panelden anında eklenir.
    /// </summary>
    private static async Task SeedCatalogAsync(AppDbContext context)
    {
        var versionSetting = await context.SiteSettings.FirstOrDefaultAsync(x => x.Key == CatalogSeedVersionKey);
        if (versionSetting != null && int.TryParse(versionSetting.Value, out var appliedVersion) && appliedVersion >= CatalogSeedVersion)
            return;

        var catalog = new Dictionary<string, Dictionary<string, string[]>>
        {
            ["Audi"] = new()
            {
                ["A3"] = new[] { "A3 Sportback 35 TFSI", "A3 Sedan 35 TFSI" },
                ["A4"] = new[] { "A4 40 TDI quattro", "A4 Sedan 40 TFSI" },
                ["A5"] = new[] { "A5 Sportback 40 TFSI" },
                ["A6"] = new[] { "A6 40 TDI quattro", "A6 45 TFSI quattro" },
                ["A7"] = new[] { "A7 Sportback 50 TDI quattro" },
                ["A8"] = new[] { "A8 L 50 TDI quattro" },
                ["Q3"] = new[] { "Q3 35 TFSI", "Q3 Sportback 35 TFSI" },
                ["Q5"] = new[] { "Q5 40 TDI quattro" },
                ["Q7"] = new[] { "Q7 50 TDI quattro" },
                ["Q8"] = new[] { "Q8 50 TDI quattro" },
                ["e-tron GT"] = new[] { "e-tron GT quattro", "RS e-tron GT" },
                ["RS"] = new[] { "RS 3", "RS 6 Avant", "RS Q8" }
            },
            ["BMW"] = new()
            {
                ["1 Serisi"] = new[] { "116i", "118i M Sport" },
                ["2 Serisi"] = new[] { "218i Gran Coupe", "220i Coupe M Sport" },
                ["3 Serisi"] = new[] { "320i", "320i M Sport", "320d xDrive", "M340i xDrive" },
                ["4 Serisi"] = new[] { "420i Gran Coupe M Sport", "430i Cabrio" },
                ["5 Serisi"] = new[] { "520i", "520d xDrive M Sport", "530e" },
                ["7 Serisi"] = new[] { "740d xDrive", "i7 xDrive60" },
                ["X1"] = new[] { "sDrive18i", "xDrive23d" },
                ["X3"] = new[] { "xDrive20d", "M40i" },
                ["X5"] = new[] { "xDrive40i M Sport", "xDrive50e" },
                ["X6"] = new[] { "xDrive40i M Sport" },
                ["X7"] = new[] { "xDrive40d" },
                ["M Serisi"] = new[] { "M2", "M3 Competition", "M4 Competition", "M5" },
                ["i Serisi"] = new[] { "i4 eDrive40", "iX xDrive50" }
            },
            ["Mercedes-Benz"] = new()
            {
                ["A Serisi"] = new[] { "A 200 AMG", "A 180 d" },
                ["C Serisi"] = new[] { "C 200 AMG", "C 220 d 4MATIC", "C 300 e" },
                ["E Serisi"] = new[] { "E 200 AMG", "E 220 d 4MATIC", "E 300 e" },
                ["S Serisi"] = new[] { "S 400 d 4MATIC", "S 580 4MATIC" },
                ["SL"] = new[] { "SL 43 AMG", "SL 63 AMG 4MATIC+" },
                ["CLA"] = new[] { "CLA 200 AMG" },
                ["CLE"] = new[] { "CLE 200 Coupe" },
                ["GLA"] = new[] { "GLA 200 AMG" },
                ["GLC"] = new[] { "GLC 220 d 4MATIC", "GLC 300 e 4MATIC" },
                ["GLE"] = new[] { "GLE 350 de 4MATIC", "GLE 450 4MATIC" },
                ["GLS"] = new[] { "GLS 450 4MATIC" },
                ["G Serisi"] = new[] { "G 400 d", "G 63 AMG" },
                ["EQ"] = new[] { "EQE 350+", "EQS 450+" },
                ["AMG GT"] = new[] { "AMG GT 63 4MATIC+" }
            },
            ["Porsche"] = new()
            {
                ["911"] = new[] { "911 Carrera", "911 Carrera S", "911 Turbo S", "911 GT3" },
                ["Cayenne"] = new[] { "Cayenne", "Cayenne E-Hybrid", "Cayenne Coupe" },
                ["Macan"] = new[] { "Macan", "Macan 4 Electric" },
                ["Panamera"] = new[] { "Panamera 4", "Panamera 4 E-Hybrid" },
                ["Taycan"] = new[] { "Taycan", "Taycan 4S", "Taycan Turbo" }
            },
            ["Land Rover"] = new()
            {
                ["Range Rover"] = new[] { "Range Rover 3.0 D350 Autobiography" },
                ["Range Rover Sport"] = new[] { "Range Rover Sport 3.0 D300 Dynamic SE" },
                ["Range Rover Velar"] = new[] { "Velar 2.0 P250 R-Dynamic" },
                ["Range Rover Evoque"] = new[] { "Evoque 1.5 P300e" },
                ["Defender"] = new[] { "Defender 110 D250", "Defender 90 P400" },
                ["Discovery"] = new[] { "Discovery 3.0 D300" }
            },
            ["Volvo"] = new()
            {
                ["S60"] = new[] { "S60 B4 Ultimate" },
                ["S90"] = new[] { "S90 B5 Ultimate" },
                ["XC40"] = new[] { "XC40 B4 Plus" },
                ["XC60"] = new[] { "XC60 B5 AWD Ultimate", "XC60 T8 Recharge" },
                ["XC90"] = new[] { "XC90 B5 AWD Ultimate" }
            },
            ["Volkswagen"] = new()
            {
                ["Golf"] = new[] { "Golf 1.5 eTSI R-Line", "Golf GTI" },
                ["Passat"] = new[] { "Passat 1.5 eTSI Business" },
                ["Tiguan"] = new[] { "Tiguan 1.5 eTSI R-Line" },
                ["Touareg"] = new[] { "Touareg 3.0 V6 TDI R-Line" }
            },
            ["Tesla"] = new()
            {
                ["Model 3"] = new[] { "Model 3 Standard Range", "Model 3 Long Range AWD" },
                ["Model Y"] = new[] { "Model Y RWD", "Model Y Long Range AWD" },
                ["Model S"] = new[] { "Model S Plaid" },
                ["Model X"] = new[] { "Model X Plaid" }
            },
            ["MG"] = new()
            {
                ["MG7"] = new[] { "MG7 1.5 Passion", "MG7 1.5 Excellence", "MG7 1.5 Excellence Red Edition" }
            },
            ["Mini"] = new()
            {
                ["Cooper"] = new[] { "Cooper S", "Cooper SE" },
                ["Countryman"] = new[] { "Countryman S ALL4" }
            },
            ["Lexus"] = new()
            {
                ["ES"] = new[] { "ES 300h" },
                ["NX"] = new[] { "NX 350h" },
                ["RX"] = new[] { "RX 450h+" }
            },
            ["Jaguar"] = new()
            {
                ["F-Pace"] = new[] { "F-Pace P250 R-Dynamic" },
                ["F-Type"] = new[] { "F-Type P450 R-Dynamic" }
            },
            ["Maserati"] = new()
            {
                ["Ghibli"] = new[] { "Ghibli GT" },
                ["Levante"] = new[] { "Levante GT" },
                ["Grecale"] = new[] { "Grecale GT", "Grecale Trofeo" }
            },
            ["Bentley"] = new()
            {
                ["Bentayga"] = new[] { "Bentayga V8" },
                ["Continental GT"] = new[] { "Continental GT V8" }
            }
        };

        var brands = await context.Brands.IgnoreQueryFilters()
            .Include(x => x.Series).ThenInclude(x => x.Models)
            .ToListAsync();
        // Panelde adı değişen kaydın eski slug'ı; seed'deki eski ad bu kayda karşılık gelir, yeniden eklenmez.
        var redirects = await context.SlugRedirects.ToListAsync();

        // 1) SpareParts'tan gelen markalar (logolarıyla) ve seriler
        for (var i = 0; i < CatalogSeedData.Brands.Length; i++)
        {
            var (name, slug) = CatalogSeedData.Brands[i];
            var brand = FindOrAddBrand(context, brands, redirects, name, slug);
            brand.LogoPath ??= $"{BrandLogoFolder}/{slug}.png";
            brand.SortOrder = i + 1;

            if (CatalogSeedData.SeriesByBrandSlug.TryGetValue(slug, out var seriesNames))
            {
                foreach (var seriesName in seriesNames)
                    FindOrAddSeries(brand, redirects, seriesName);
            }
        }

        // 2) Motor + paket seviyesindeki örnek modeller
        foreach (var (brandName, seriesMap) in catalog)
        {
            var brand = FindOrAddBrand(context, brands, redirects, brandName, SlugHelper.Generate(brandName));
            foreach (var (seriesName, models) in seriesMap)
            {
                var series = FindOrAddSeries(brand, redirects, seriesName);
                foreach (var modelName in models)
                {
                    var modelSlug = SlugHelper.Generate(modelName);
                    var renamed = series.Id != 0 && redirects.Any(x =>
                        x.EntityType == SlugEntityType.CarModel && x.ParentId == series.Id && x.OldSlug == modelSlug);
                    if (!renamed && series.Models.All(x => x.Slug != modelSlug))
                        series.Models.Add(new CarModelEntity { Name = modelName, Slug = modelSlug });
                }
            }
        }

        // Seriler alfabetik sıralanır; sayılar doğal sırada (3 Serisi < 10, 206 < 2008).
        foreach (var brand in brands)
        {
            var ordered = brand.Series.OrderBy(x => x.Name, NameOrder).ToList();
            for (var i = 0; i < ordered.Count; i++)
                ordered[i].SortOrder = i + 1;
        }

        if (versionSetting == null)
            context.SiteSettings.Add(new SiteSettingEntity { Key = CatalogSeedVersionKey, Value = CatalogSeedVersion.ToString() });
        else
            versionSetting.Value = CatalogSeedVersion.ToString();

        await context.SaveChangesAsync();
    }

    private static BrandEntity FindOrAddBrand(AppDbContext context, List<BrandEntity> brands, List<SlugRedirectEntity> redirects,
        string name, string slug)
    {
        var brand = brands.FirstOrDefault(x => x.Slug == slug);
        if (brand != null)
            return brand;

        var redirect = redirects.FirstOrDefault(x => x.EntityType == SlugEntityType.Brand && x.OldSlug == slug);
        brand = redirect == null ? null : brands.FirstOrDefault(x => x.Id == redirect.EntityId);
        if (brand != null)
            return brand;

        brand = new BrandEntity { Name = name, Slug = slug };
        brands.Add(brand);
        context.Brands.Add(brand);
        return brand;
    }

    private static SeriesEntity FindOrAddSeries(BrandEntity brand, List<SlugRedirectEntity> redirects, string name)
    {
        var slug = SlugHelper.Generate(name);
        var series = brand.Series.FirstOrDefault(x => x.Slug == slug);
        if (series != null)
            return series;

        var redirect = brand.Id == 0 ? null : redirects.FirstOrDefault(x =>
            x.EntityType == SlugEntityType.Series && x.ParentId == brand.Id && x.OldSlug == slug);
        series = redirect == null ? null : brand.Series.FirstOrDefault(x => x.Id == redirect.EntityId);
        if (series != null)
            return series;

        series = new SeriesEntity { Name = name, Slug = slug };
        brand.Series.Add(series);
        return series;
    }

    /// <summary>
    /// Varsayılan ayarlar anahtar bazında: eksik anahtar eklenir, var olana (boş değer dahil) dokunulmaz.
    /// Ayar satırları panelden silinmez (SiteSettingService yalnız değer yazar), bu yüzden işarete gerek yok.
    /// </summary>
    private static async Task SeedSettingsAsync(AppDbContext context)
    {
        var defaults = new Dictionary<string, string>
        {
            [nameof(SiteSettingsModel.GalleryName)] = "Fatih Taşçı Otomotiv",
            [nameof(SiteSettingsModel.WorkingHours)] = "Pazartesi - Cumartesi 09:00 - 19:00"
        };

        var keys = defaults.Keys.ToList();
        var existing = await context.SiteSettings.Where(x => keys.Contains(x.Key)).Select(x => x.Key).ToListAsync();
        var missing = defaults.Where(x => !existing.Contains(x.Key)).ToList();
        if (missing.Count == 0)
            return;

        context.SiteSettings.AddRange(missing.Select(x => new SiteSettingEntity { Key = x.Key, Value = x.Value }));
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Galeri atmosferi için başlangıç sahneleri (fotoğrafsız). Fotoğraflar panelden yüklenir;
    /// fotoğrafı olmayan sahne vitrinde görünmez. Kayıt bazında (başlığa göre); silinen sahne geri gelmez.
    /// </summary>
    private static async Task SeedShowroomScenesAsync(AppDbContext context)
    {
        var scenes = new (string Tag, string Title)[]
        {
            ("Dış cephe & vitrin", "Showroom ve Vitrin"),
            ("Sergi salonu", "Işıklandırılmış Sergi Salonu"),
            ("Misafir alanı", "Kahve ve Birebir Görüşme"),
            ("Hazırlık", "Detaylı Bakım ve Hazırlık"),
            ("Ekspertiz", "Kontrol ve Ekspertiz Alanı"),
            ("Teslimat", "Teslimat Sahnesi")
        };

        var existing = await context.ShowroomScenes.IgnoreQueryFilters().Select(x => new { x.Title, x.SortOrder }).ToListAsync();
        var nextOrder = existing.Count == 0 ? 0 : existing.Max(x => x.SortOrder) + 1;
        await SeedListAsync(context, SeededShowroomScenesKey, scenes.Select(x => x.Title), existing.Select(x => x.Title), title =>
        {
            var scene = scenes.First(x => x.Title == title);
            context.ShowroomScenes.Add(new ShowroomSceneEntity { Tag = scene.Tag, Title = scene.Title, SortOrder = nextOrder++ });
        });
    }

    /// <summary>
    /// Kayıt bazında seed: işarette (SiteSettings, markerKey) olmayan anahtarlar için, aynı adla kayıt yoksa add
    /// çağrılır; eklenen ya da zaten var bulunan anahtar işarete yazılır. Böylece işaretli kayıt panelde silinse ya da
    /// adı değişse de geri gelmez, listeye sonradan eklenen kayıt mevcut veritabanına bir kez eklenir.
    /// İşaretsiz eski veritabanında ilk çalışmada aynı adla bulunan kayıtlar yalnız işaretlenir.
    /// </summary>
    private static async Task SeedListAsync(AppDbContext context, string markerKey, IEnumerable<string> keys,
        IEnumerable<string> existingNames, Action<string> add)
    {
        var marker = await context.SiteSettings.FirstOrDefaultAsync(x => x.Key == markerKey);
        var seeded = new HashSet<string>((marker?.Value ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries), KeyComparer);
        var existing = new HashSet<string>(existingNames.Where(x => x != null), KeyComparer);
        var changed = false;

        foreach (var key in keys)
        {
            if (!seeded.Add(key))
                continue;
            changed = true;
            if (existing.Add(key))
                add(key);
        }

        if (!changed)
            return;

        var value = string.Join('\n', seeded.OrderBy(x => x, KeyComparer));
        if (value.Length > SettingValueMaxLength)
            throw new InvalidOperationException($"Seed işareti {markerKey} {SettingValueMaxLength} karakteri aşıyor.");

        if (marker == null)
            context.SiteSettings.Add(new SiteSettingEntity { Key = markerKey, Value = value });
        else
            marker.Value = value;
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Geliştirme ortamı için örnek teslimatlar: Unsplash lisanslı stok fotoğraflar (Server/Assets/Seed/Deliveries,
    /// kaynakları KAYNAK.md'de) panel yüklemesiyle aynı yoldan işlenir. Yayında çalışmaz: stok fotoğraf ve uydurma
    /// müşteri adı canlı sitede gerçek teslimat gibi görünür ve ziyaretçiyi yanıltır.
    /// </summary>
    private static async Task SeedDemoDeliveriesAsync(AppDbContext context, IMediaStorage storage, string folder, ILogger logger)
    {
        if (string.IsNullOrEmpty(folder) || storage == null || !Directory.Exists(folder))
            return;
        var marker = await context.SiteSettings.FirstOrDefaultAsync(x => x.Key == DemoDeliveriesSeededKey);
        if (marker != null && int.TryParse(marker.Value, out var appliedVersion) && appliedVersion >= DemoDeliveriesSeedVersion)
            return;
        if (marker != null)
            await RemoveDemoDeliveriesAsync(context, storage, logger);

        var samples = new (string File, string Vehicle, string Customer, string City, int DaysAgo)[]
        {
            ("01-rav4-cift.jpg", "2022 Toyota RAV4 2.5 Hybrid Passion", "Seda ve Onur K.", "İstanbul", 3),
            ("02-jimny-alanya.jpg", "2021 Suzuki Jimny 1.5 GLX AllGrip", "Derya S.", "Antalya", 7),
            ("03-audi-a3-ankara.jpg", "2023 Audi A3 Sedan 35 TFSI S line", "Burak E.", "Ankara", 12),
            ("04-anahtar-alan-musteri.jpg", "2023 Toyota C-HR 1.8 Hybrid Passion X", "Serkan B.", "Kocaeli", 18),
            ("05-arabasina-sarilan.jpg", "2022 Peugeot 3008 1.5 BlueHDi GT", "Okan Y.", "Bursa", 25),
            ("06-golf-zonguldak.jpg", "2017 Volkswagen Golf 1.4 TSI Highline", "Mehmet A.", "Zonguldak", 33),
            ("07-aile-anahtar.jpg", "2022 Volvo XC90 B5 Inscription", "Murat K.", "Eskişehir", 44),
            ("08-civic-izmir.jpg", "2000 Honda Civic 1.6 VTi", "Emre T.", "İzmir", 56)
        };

        var written = new List<(bool IsPublic, string Path)>();
        var added = 0;
        try
        {
            foreach (var sample in samples)
            {
                var path = Path.Combine(folder, sample.File);
                if (!File.Exists(path))
                {
                    logger.LogWarning("Örnek teslimat fotoğrafı bulunamadı: {Path}", path);
                    continue;
                }

                var delivery = new DeliveryEntity
                {
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

            if (marker == null)
                context.SiteSettings.Add(new SiteSettingEntity { Key = DemoDeliveriesSeededKey, Value = DemoDeliveriesSeedVersion.ToString() });
            else
                marker.Value = DemoDeliveriesSeedVersion.ToString();
            await context.SaveChangesAsync();
        }
        catch
        {
            // Kaydedilemeyen teslimatların dosyaları silinir; işaret yazılmadığı için bir sonraki açılışta yeniden denenir.
            foreach (var (isPublic, path) in written)
            {
                try
                {
                    if (isPublic)
                        storage.DeletePublic(path);
                    else
                        storage.DeletePrivate(path);
                }
                catch (IOException ex)
                {
                    logger.LogWarning(ex, "Dosya silinemedi: {Path}", path);
                }
            }
            throw;
        }

        logger.LogInformation("{Count} örnek teslimat eklendi (yalnız geliştirme).", added);
    }

    /// <summary>
    /// Önceki sürümün örnek teslimatlarını dosyalarıyla kaldırır. Seed'in eklediği kayıtların oluşturanı yoktur
    /// (panelden eklenenlerde kullanıcı yazılır), böylece panelde eklenen teslimatlara dokunulmaz.
    /// </summary>
    private static async Task RemoveDemoDeliveriesAsync(AppDbContext context, IMediaStorage storage, ILogger logger)
    {
        var old = await context.Deliveries.IgnoreQueryFilters().Where(x => x.CreatedBy == null).ToListAsync();
        if (old.Count == 0)
            return;

        foreach (var delivery in old)
        {
            foreach (var (isPublic, path) in new[] { (false, delivery.OriginalPath), (true, delivery.MediumPath), (true, delivery.LargePath) })
            {
                if (string.IsNullOrEmpty(path))
                    continue;
                try
                {
                    if (isPublic)
                        storage.DeletePublic(path);
                    else
                        storage.DeletePrivate(path);
                }
                catch (IOException ex)
                {
                    logger.LogWarning(ex, "Dosya silinemedi: {Path}", path);
                }
            }
        }

        context.Deliveries.RemoveRange(old);
        await context.SaveChangesAsync();
        logger.LogInformation("Önceki {Count} örnek teslimat kaldırıldı.", old.Count);
    }

    /// <summary>
    /// Geliştirme ortamı için yayında örnek araçlar (liste Seed/DemoVehicleSeedData.cs). Fotoğraflar
    /// Server/Assets/Seed/Vehicles/{klasör} altındadır (kaynakları KAYNAK.md'de), dosya adı sırasıyla eklenir, ilki kapaktır;
    /// panel yüklemesiyle aynı yoldan işlenir (filigransız).
    /// Yayında çalışmaz: fotoğraflar başka galerinin ilanından, üzerlerinde o galerinin ve portalın filigranı var.
    /// </summary>
    private static async Task SeedDemoVehiclesAsync(AppDbContext context, IMediaStorage storage, string folder, ILogger logger)
    {
        if (storage == null || !Directory.Exists(folder))
            return;

        var marker = await context.SiteSettings.FirstOrDefaultAsync(x => x.Key == DemoVehiclesSeededKey);
        var seeded = (marker?.Value ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        var pending = DemoVehicleSeedData.Create().Where(x => !seeded.Contains(x.Folder)).ToList();
        if (pending.Count == 0)
            return;

        var colors = await context.Colors.ToListAsync();
        var features = await context.Features.ToListAsync();
        var writtenPublic = new List<string>();
        var writtenPrivate = new List<string>();
        var added = 0;

        try
        {
            foreach (var sample in pending)
            {
                var photoFolder = Path.Combine(folder, sample.Folder);
                var photos = Directory.Exists(photoFolder)
                    ? Directory.GetFiles(photoFolder)
                        .Where(x => DemoVehiclePhotoExtensions.Contains(Path.GetExtension(x).ToLowerInvariant()))
                        .OrderBy(x => Path.GetFileName(x), StringComparer.OrdinalIgnoreCase)
                        .ToList()
                    : new List<string>();
                if (photos.Count == 0)
                {
                    logger.LogWarning("Örnek araç fotoğrafı bulunamadı: {Path}", photoFolder);
                    continue;
                }

                var brandSlug = SlugHelper.Generate(sample.Brand);
                var seriesSlug = SlugHelper.Generate(sample.Series);
                var modelSlug = SlugHelper.Generate(sample.Model);
                var model = await context.CarModels
                    .Include(x => x.Series).ThenInclude(x => x.Brand)
                    .FirstOrDefaultAsync(x => x.Slug == modelSlug && x.Series.Slug == seriesSlug && x.Series.Brand.Slug == brandSlug);
                if (model == null)
                {
                    logger.LogWarning("Örnek araç kataloğu bulunamadı: {Brand} / {Series} / {Model}", sample.Brand, sample.Series, sample.Model);
                    continue;
                }

                var vehicle = sample.Vehicle;
                vehicle.Id = Guid.NewGuid();
                vehicle.BrandId = model.Series.BrandId;
                vehicle.SeriesId = model.SeriesId;
                vehicle.CarModelId = model.Id;
                vehicle.Slug = SlugHelper.ForVehicle(vehicle.Year, model.Series.Brand.Name, model.Name);
                vehicle.ExteriorColorId = colors.FirstOrDefault(x => x.Name == sample.Color)?.Id;
                vehicle.Status = VehicleStatus.Published;
                vehicle.PublishedAt = DateTime.Now;

                foreach (var part in Enum.GetValues<BodyPart>())
                    vehicle.BodyParts.Add(new VehicleBodyPartEntity { Part = part, State = BodyPartState.Original });

                foreach (var feature in features.Where(x => sample.Features.Contains(x.Name)))
                    vehicle.Features.Add(new VehicleFeatureEntity { FeatureId = feature.Id });

                for (var i = 0; i < photos.Count; i++)
                {
                    await using var stream = File.OpenRead(photos[i]);
                    var media = await MediaService.WriteImageAsync(storage, vehicle.Id, stream, Path.GetExtension(photos[i]), null, writtenPublic, writtenPrivate);
                    if (media == null)
                    {
                        logger.LogWarning("Örnek araç fotoğrafı okunamadı: {Path}", photos[i]);
                        continue;
                    }

                    media.SortOrder = vehicle.Media.Count;
                    media.IsCover = vehicle.Media.Count == 0;
                    vehicle.Media.Add(media);
                }

                if (vehicle.Media.Count == 0)
                    continue;

                context.Vehicles.Add(vehicle);
                seeded.Add(sample.Folder);
                added++;
            }

            var value = string.Join(",", seeded);
            if (marker == null)
                context.SiteSettings.Add(new SiteSettingEntity { Key = DemoVehiclesSeededKey, Value = value });
            else
                marker.Value = value;
            await context.SaveChangesAsync();
        }
        catch
        {
            // Kaydedilemeyen araçların dosyaları silinir; işaret yazılmadığı için bir sonraki açılışta yeniden denenir.
            foreach (var path in writtenPublic)
                TryDeleteFile(() => storage.DeletePublic(path), path, logger);
            foreach (var path in writtenPrivate)
                TryDeleteFile(() => storage.DeletePrivate(path), path, logger);
            throw;
        }

        if (added > 0)
            logger.LogInformation("{Count} örnek araç eklendi (yalnız geliştirme).", added);
    }

    private static void TryDeleteFile(Action delete, string path, ILogger logger)
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
