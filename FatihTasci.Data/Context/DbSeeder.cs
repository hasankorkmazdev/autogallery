using FatihTasci.Core.Entity.Catalog;
using FatihTasci.Core.Entity.Common;
using FatihTasci.Core.Entity.Identity;
using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Enums;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain.Setting;
using FatihTasci.Data.Context.Seed;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FatihTasci.Data.Context;

/// <summary>
/// Başlangıç verisi. Her bölüm yalnızca ilgili tablo boşsa çalışır, bu yüzden her açılışta güvenle çağrılabilir.
/// Yönetici bilgileri koda yazılmaz, SeedAdmin ayarından okunur.
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
    private const int CatalogSeedVersion = 2;

    private static readonly StringComparer NameOrder =
        StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), CompareOptions.IgnoreCase | CompareOptions.NumericOrdering);

    public static async Task SeedAsync(AppDbContext context, IConfiguration configuration, IPasswordHasher<UserEntity> passwordHasher, ILogger logger)
    {
        await SeedAdminAsync(context, configuration, passwordHasher, logger);
        await SeedColorsAsync(context);
        await SeedFeaturesAsync(context);
        await SeedCatalogAsync(context);
        await SeedSettingsAsync(context);
        await SeedShowroomScenesAsync(context);
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
        if (await context.Colors.AnyAsync())
            return;

        var colors = new (string Name, string Hex)[]
        {
            ("Beyaz", "#F5F5F5"), ("Siyah", "#111111"), ("Gri", "#808080"), ("Gümüş Gri", "#B8B8B8"),
            ("Füme", "#4A4D52"), ("Lacivert", "#1B2A4A"), ("Mavi", "#2F5DA8"), ("Kırmızı", "#B3202A"),
            ("Bordo", "#6B1E2B"), ("Kahverengi", "#5C4033"), ("Bej", "#D8C8A8"), ("Şampanya", "#C8A96A"),
            ("Yeşil", "#2F5D3A"), ("Turuncu", "#D2691E"), ("Sarı", "#E0B000"), ("Mor", "#5B3A7A")
        };

        context.Colors.AddRange(colors.Select((c, i) => new ColorEntity { Name = c.Name, HexCode = c.Hex, SortOrder = i }));
        await context.SaveChangesAsync();
    }

    private static async Task SeedFeaturesAsync(AppDbContext context)
    {
        if (await context.Features.AnyAsync())
            return;

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

        foreach (var (category, names) in groups)
        {
            context.Features.AddRange(names.Select((name, i) => new FeatureEntity
            {
                Category = category,
                Name = name,
                SortOrder = i
            }));
        }

        await context.SaveChangesAsync();
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

        // 1) SpareParts'tan gelen markalar (logolarıyla) ve seriler
        for (var i = 0; i < CatalogSeedData.Brands.Length; i++)
        {
            var (name, slug) = CatalogSeedData.Brands[i];
            var brand = FindOrAddBrand(context, brands, name, slug);
            brand.LogoPath ??= $"{BrandLogoFolder}/{slug}.png";
            brand.SortOrder = i + 1;

            if (CatalogSeedData.SeriesByBrandSlug.TryGetValue(slug, out var seriesNames))
            {
                foreach (var seriesName in seriesNames)
                    FindOrAddSeries(brand, seriesName);
            }
        }

        // 2) Motor + paket seviyesindeki örnek modeller
        foreach (var (brandName, seriesMap) in catalog)
        {
            var brand = FindOrAddBrand(context, brands, brandName, SlugHelper.Generate(brandName));
            foreach (var (seriesName, models) in seriesMap)
            {
                var series = FindOrAddSeries(brand, seriesName);
                foreach (var modelName in models)
                {
                    var modelSlug = SlugHelper.Generate(modelName);
                    if (series.Models.All(x => x.Slug != modelSlug))
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

    private static BrandEntity FindOrAddBrand(AppDbContext context, List<BrandEntity> brands, string name, string slug)
    {
        var brand = brands.FirstOrDefault(x => x.Slug == slug);
        if (brand != null)
            return brand;

        brand = new BrandEntity { Name = name, Slug = slug };
        brands.Add(brand);
        context.Brands.Add(brand);
        return brand;
    }

    private static SeriesEntity FindOrAddSeries(BrandEntity brand, string name)
    {
        var slug = SlugHelper.Generate(name);
        var series = brand.Series.FirstOrDefault(x => x.Slug == slug);
        if (series != null)
            return series;

        series = new SeriesEntity { Name = name, Slug = slug };
        brand.Series.Add(series);
        return series;
    }

    private static async Task SeedSettingsAsync(AppDbContext context)
    {
        if (await context.SiteSettings.AnyAsync(x => x.Key == nameof(SiteSettingsModel.GalleryName)))
            return;

        context.SiteSettings.AddRange(
            new SiteSettingEntity { Key = nameof(SiteSettingsModel.GalleryName), Value = "Premium Oto Galeri" },
            new SiteSettingEntity { Key = nameof(SiteSettingsModel.WorkingHours), Value = "Pazartesi - Cumartesi 09:00 - 19:00" });
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Galeri atmosferi için başlangıç sahneleri (fotoğrafsız). Fotoğraflar panelden yüklenir;
    /// fotoğrafı olmayan sahne vitrinde görünmez. Silinmiş sahneler de sayılır, böylece yeniden eklenmez.
    /// </summary>
    private static async Task SeedShowroomScenesAsync(AppDbContext context)
    {
        if (await context.ShowroomScenes.IgnoreQueryFilters().AnyAsync())
            return;

        var scenes = new (string Tag, string Title)[]
        {
            ("Dış cephe & vitrin", "Showroom ve Vitrin"),
            ("Sergi salonu", "Işıklandırılmış Sergi Salonu"),
            ("Misafir alanı", "Kahve ve Birebir Görüşme"),
            ("Hazırlık", "Detaylı Bakım ve Hazırlık"),
            ("Ekspertiz", "Kontrol ve Ekspertiz Alanı"),
            ("Teslimat", "Teslimat Sahnesi")
        };

        context.ShowroomScenes.AddRange(scenes.Select((s, i) => new ShowroomSceneEntity { Tag = s.Tag, Title = s.Title, SortOrder = i }));
        await context.SaveChangesAsync();
    }
}
