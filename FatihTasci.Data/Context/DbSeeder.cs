using FatihTasci.Core.Entity.Catalog;
using FatihTasci.Core.Entity.Common;
using FatihTasci.Core.Entity.Identity;
using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Enums;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain.Setting;
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
    public static async Task SeedAsync(AppDbContext context, IConfiguration configuration, IPasswordHasher<UserEntity> passwordHasher, ILogger logger)
    {
        await SeedAdminAsync(context, configuration, passwordHasher, logger);
        await SeedColorsAsync(context);
        await SeedFeaturesAsync(context);
        await SeedCatalogAsync(context);
        await SeedSettingsAsync(context);
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
    /// Yaygın premium markalar ve serileri. Model listesi örnek niteliğindedir; eksik model
    /// araç girerken panelden anında eklenir (PROJE_DOKUMANI §3.1, §9 açık soru: katalog kapsamı).
    /// </summary>
    private static async Task SeedCatalogAsync(AppDbContext context)
    {
        if (await context.Brands.IgnoreQueryFilters().AnyAsync())
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

        var brandOrder = 0;
        foreach (var (brandName, seriesMap) in catalog)
        {
            var brand = new BrandEntity { Name = brandName, Slug = SlugHelper.Generate(brandName), SortOrder = brandOrder++ };
            var seriesOrder = 0;
            foreach (var (seriesName, models) in seriesMap)
            {
                var series = new SeriesEntity { Name = seriesName, Slug = SlugHelper.Generate(seriesName), SortOrder = seriesOrder++ };
                foreach (var modelName in models)
                    series.Models.Add(new CarModelEntity { Name = modelName, Slug = SlugHelper.Generate(modelName) });
                brand.Series.Add(series);
            }
            context.Brands.Add(brand);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedSettingsAsync(AppDbContext context)
    {
        if (await context.SiteSettings.AnyAsync())
            return;

        context.SiteSettings.AddRange(
            new SiteSettingEntity { Key = nameof(SiteSettingsModel.GalleryName), Value = "Premium Oto Galeri" },
            new SiteSettingEntity { Key = nameof(SiteSettingsModel.WorkingHours), Value = "Pazartesi - Cumartesi 09:00 - 19:00" });
        await context.SaveChangesAsync();
    }
}
