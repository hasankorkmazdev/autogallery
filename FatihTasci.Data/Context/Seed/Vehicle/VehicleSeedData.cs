using FatihTasci.Core.Enums;

namespace FatihTasci.Data.Context.Seed.Vehicle;

/// <summary>
/// Araç renkleri ve donanım listesi (VehicleSeeder). Sıra listedeki sıradır (donanımda kategori içinde). Yeni kayda o
/// tabloda kullanılmamış bir Id verilir (seed Id'leri 100000'in altında).
/// </summary>
internal static class VehicleSeedData
{
    public static readonly (int Id, string Name, string Hex)[] Colors =
    [
        (1, "Beyaz", "#F5F5F5"), (2, "Siyah", "#111111"), (3, "Gri", "#808080"), (4, "Gümüş Gri", "#B8B8B8"),
        (5, "Füme", "#4A4D52"), (6, "Lacivert", "#1B2A4A"), (7, "Mavi", "#2F5DA8"), (8, "Kırmızı", "#B3202A"),
        (9, "Bordo", "#6B1E2B"), (10, "Kahverengi", "#5C4033"), (11, "Bej", "#D8C8A8"), (12, "Şampanya", "#C8A96A"),
        (13, "Yeşil", "#2F5D3A"), (14, "Turuncu", "#D2691E"), (15, "Sarı", "#E0B000"), (16, "Mor", "#5B3A7A")
    ];

    public static readonly (FeatureCategory Category, (int Id, string Name)[] Features)[] Features =
    [
        (FeatureCategory.Safety,
        [
            (1, "ABS"), (2, "ESP"), (3, "ASR (Çekiş Kontrolü)"), (4, "Sürücü Hava Yastığı"), (5, "Yolcu Hava Yastığı"),
            (6, "Yan Hava Yastıkları"), (7, "Perde Hava Yastıkları"), (8, "Diz Hava Yastığı"), (9, "Isofix"),
            (10, "Kör Nokta Uyarısı"), (11, "Şerit Takip"), (12, "Şeritte Tutma Asistanı"), (13, "Ön Çarpışma Uyarısı"),
            (14, "Acil Fren Asistanı"), (15, "Yorgunluk Tespiti"), (16, "Yokuş Kalkış Desteği"), (17, "Lastik Basınç İzleme"),
            (18, "Gece Görüş"), (19, "Merkezi Kilit"), (20, "Immobilizer"), (21, "Alarm"), (22, "Çocuk Kilidi")
        ]),
        (FeatureCategory.Interior,
        [
            (23, "Deri Koltuk"), (24, "Kumaş Koltuk"), (25, "Alcantara"), (26, "Elektrikli Ön Koltuklar"),
            (27, "Hafızalı Koltuklar"), (28, "Isıtmalı Ön Koltuklar"), (29, "Isıtmalı Arka Koltuklar"),
            (30, "Soğutmalı Koltuklar"), (31, "Masajlı Koltuklar"), (32, "Manuel Klima"), (33, "Otomatik Klima"),
            (34, "Çift Bölgeli Klima"), (35, "Hız Sabitleyici"), (36, "Adaptif Hız Sabitleyici"), (37, "Head-up Display"),
            (38, "Dijital Gösterge Paneli"), (39, "Isıtmalı Direksiyon"), (40, "Fonksiyonel Direksiyon"),
            (41, "Anahtarsız Giriş ve Çalıştırma"), (42, "Start/Stop"), (43, "Elektrikli Ön Camlar"),
            (44, "Elektrikli Arka Camlar"), (45, "Ambiyans Aydınlatma"), (46, "Elektrikli Bagaj Kapağı"),
            (47, "Yol Bilgisayarı"), (48, "Geri Görüş Kamerası"), (49, "360° Kamera"), (50, "Otomatik Park Asistanı")
        ]),
        (FeatureCategory.Exterior,
        [
            (51, "Sunroof"), (52, "Panoramik Cam Tavan"), (53, "LED Far"), (54, "Adaptif / Matrix LED Far"), (55, "Xenon Far"),
            (56, "Far Yıkama"), (57, "Sis Farı"), (58, "Far Sensörü"), (59, "Yağmur Sensörü"), (60, "Ön Park Sensörü"),
            (61, "Arka Park Sensörü"), (62, "Elektrikli Aynalar"), (63, "Isıtmalı Aynalar"), (64, "Elektrikli Katlanır Aynalar"),
            (65, "Otomatik Kararan Ayna"), (66, "Alaşım Jant"), (67, "Römork Demiri"), (68, "Havalı Süspansiyon"),
            (69, "Spoiler"), (70, "Karartılmış Arka Camlar")
        ]),
        (FeatureCategory.Multimedia,
        [
            (71, "Bluetooth"), (72, "USB / AUX"), (73, "Apple CarPlay"), (74, "Android Auto"), (75, "Navigasyon"),
            (76, "Kablosuz Şarj"), (77, "Premium Ses Sistemi"), (78, "Dokunmatik Ekran"), (79, "Sesli Komut"),
            (80, "Arka Eğlence Sistemi")
        ])
    ];
}
