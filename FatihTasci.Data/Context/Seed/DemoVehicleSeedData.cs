using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Enums;

namespace FatihTasci.Data.Context.Seed;

/// <summary>
/// Geliştirme ortamındaki örnek araçlar (DbSeeder.SeedDemoVehiclesAsync). Folder, Server/Assets/Seed/Vehicles altındaki
/// fotoğraf klasörüdür ve örneğin kimliğidir (bir kez eklenir). Katalog adı (Brand / Series / Model) katalog seed'inde
/// bulunmalıdır; Color ve Features adları renk / donanım seed'indeki adlardır, bulunamayan atlanır.
/// Fiyatlar tahminidir; fotoğraflar ve kaynaklar Vehicles/KAYNAK.md'de.
/// </summary>
internal static class DemoVehicleSeedData
{
    /// <summary>
    /// Her çağrıda yeni entity'ler döner (seed bunları doldurup kaydeder).
    /// </summary>
    public static (string Folder, string Brand, string Series, string Model, string Color, string[] Features, VehicleEntity Vehicle)[] Create() =>
    [
        (
            "2025-mg7-red-edition", "MG", "MG7", "MG7 1.5 Excellence Red Edition", "Beyaz",
            [
                "ABS", "ESP", "ASR (Çekiş Kontrolü)", "Sürücü Hava Yastığı", "Yolcu Hava Yastığı", "Yan Hava Yastıkları",
                "Perde Hava Yastıkları", "Isofix", "Kör Nokta Uyarısı", "Şerit Takip", "Şeritte Tutma Asistanı",
                "Ön Çarpışma Uyarısı", "Acil Fren Asistanı", "Yokuş Kalkış Desteği", "Lastik Basınç İzleme", "Merkezi Kilit",
                "Immobilizer",
                "Alcantara", "Elektrikli Ön Koltuklar", "Otomatik Klima", "Çift Bölgeli Klima", "Adaptif Hız Sabitleyici",
                "Dijital Gösterge Paneli", "Fonksiyonel Direksiyon", "Anahtarsız Giriş ve Çalıştırma", "Elektrikli Ön Camlar",
                "Elektrikli Arka Camlar", "Ambiyans Aydınlatma", "Yol Bilgisayarı", "Geri Görüş Kamerası", "360° Kamera",
                "Panoramik Cam Tavan", "LED Far", "Sis Farı", "Far Sensörü", "Yağmur Sensörü", "Ön Park Sensörü",
                "Arka Park Sensörü", "Elektrikli Aynalar", "Isıtmalı Aynalar", "Elektrikli Katlanır Aynalar", "Alaşım Jant",
                "Spoiler", "Karartılmış Arka Camlar",
                "Bluetooth", "USB / AUX", "Apple CarPlay", "Android Auto", "Kablosuz Şarj", "Dokunmatik Ekran"
            ],
            new VehicleEntity
            {
                Title = "Boyasız 2025 MG MG7 1.5 Excellence Red Edition 19.000 KM",
                Year = 2025,
                Kilometer = 19_000,
                FuelType = FuelType.Gasoline,
                Transmission = TransmissionType.Automatic,
                BodyType = BodyType.Sedan,
                EngineCc = 1498,
                PowerHp = 170,
                Drivetrain = Drivetrain.FrontWheel,
                InteriorColor = "Kırmızı Alcantara",
                Condition = VehicleCondition.Used,
                PlateOrigin = PlateOrigin.Turkish,
                Price = 2_690_000m,
                Description =
                    "<p>2025 model MG7 1.5 Excellence Red Edition, 19.000 km'de; boyasız ve değişensiz.</p>" +
                    "<p>170 PS 1.5 turbo benzinli motor, 7 ileri çift kavramalı otomatik şanzıman.</p>" +
                    "<ul><li>Red Edition'a özel MG logolu kırmızı Alcantara koltuklar ve kırmızı iç ambiyans</li>" +
                    "<li>1,8 m² elektrikli açılır panoramik cam tavan</li>" +
                    "<li>19\" alaşım jantlar, aktif arka spoiler</li>" +
                    "<li>360° kamera, adaptif hız sabitleyici, kör nokta uyarısı</li></ul>"
            }
        ),
        (
            "2025-mercedes-sl43-amg", "Mercedes-Benz", "SL", "SL 43 AMG", "Mavi",
            [
                "ABS", "ESP", "ASR (Çekiş Kontrolü)", "Sürücü Hava Yastığı", "Yolcu Hava Yastığı", "Yan Hava Yastıkları",
                "Şerit Takip", "Acil Fren Asistanı", "Yorgunluk Tespiti", "Yokuş Kalkış Desteği", "Lastik Basınç İzleme",
                "Merkezi Kilit", "Immobilizer", "Alarm",
                "Deri Koltuk", "Elektrikli Ön Koltuklar", "Hafızalı Koltuklar", "Isıtmalı Ön Koltuklar", "Soğutmalı Koltuklar",
                "Otomatik Klima", "Hız Sabitleyici", "Dijital Gösterge Paneli", "Fonksiyonel Direksiyon",
                "Anahtarsız Giriş ve Çalıştırma", "Start/Stop", "Elektrikli Ön Camlar", "Ambiyans Aydınlatma", "Yol Bilgisayarı",
                "Geri Görüş Kamerası",
                "LED Far", "Far Sensörü", "Yağmur Sensörü", "Ön Park Sensörü", "Arka Park Sensörü", "Elektrikli Aynalar",
                "Isıtmalı Aynalar", "Elektrikli Katlanır Aynalar", "Otomatik Kararan Ayna", "Alaşım Jant", "Spoiler",
                "Bluetooth", "USB / AUX", "Apple CarPlay", "Android Auto", "Navigasyon", "Kablosuz Şarj", "Premium Ses Sistemi",
                "Dokunmatik Ekran", "Sesli Komut"
            ],
            new VehicleEntity
            {
                Title = "Bayi 2025 Mercedes SL43 AMG Performance AKS Soğutma",
                Year = 2025,
                Kilometer = 6_100,
                FuelType = FuelType.Gasoline,
                Transmission = TransmissionType.Automatic,
                BodyType = BodyType.Cabrio,
                EngineCc = 1991,
                PowerHp = 421,
                Drivetrain = Drivetrain.RearWheel,
                Condition = VehicleCondition.Used,
                HasWarranty = true,
                PlateOrigin = PlateOrigin.Turkish,
                Price = 12_450_000m,
                Description =
                    "<p>Bayi çıkışlı 2025 model Mercedes-AMG SL 43, 6.100 km'de; servis garantisi devam ediyor, ağır hasar kaydı yok.</p>" +
                    "<p>421 hp 2.0 turbo benzinli motor (48 V hafif hibrit), otomatik şanzıman, arkadan itiş.</p>" +
                    "<ul><li>AMG Performance donanımı, soğutmalı koltuklar</li>" +
                    "<li>Elektrikli açılır kumaş tavan</li>" +
                    "<li>Sarı AMG fren kaliperleri, çok kollu alaşım jantlar</li></ul>"
            }
        )
    ];
}
