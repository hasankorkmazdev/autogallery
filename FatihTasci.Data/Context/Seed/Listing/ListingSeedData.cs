using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Enums;

namespace FatihTasci.Data.Context.Seed.Listing;

/// <summary>
/// Galerinin gerçek ilanları (ListingSeeder), sahibinden ilanlarından elle aktarıldı. Folder, Server/Assets/Seed/Vehicles
/// altındaki fotoğraf klasörüdür (dosya adı sırası ilandaki sıradır, ilki kapak). Vehicle.Id sabittir ve kaydın kimliğidir
/// (bir kez eklenir); yeni ilana yeni Guid verilir. Brand / Series / Model katalog seed'indeki (CatalogSeedData), Color ve
/// Features renk / donanım seed'indeki (VehicleSeedData) adlardır. Donanımlar ilanın açıklamasından, fotoğraflardan ve
/// modelin standart donanımından seçildi. Boya / değişen girilmeyen parçalar sitede orijinal görünür. Vehicle.AdvisorId
/// boşsa danışman AdvisorId'dir (hesap yoksa ya da pasifse Yönetici).
/// </summary>
internal static class ListingSeedData
{
    /// <summary>
    /// İlanların satış danışmanı: Hasan Korkmaz (AccountSeedData).
    /// </summary>
    public static readonly Guid AdvisorId = new("fb30d4ee-060c-44b0-b2a4-00b8b7278a0e");

    /// <summary>
    /// Her çağrıda yeni entity'ler döner (seed bunları doldurup kaydeder).
    /// </summary>
    public static (string Folder, string Brand, string Series, string Model, string Color, string[] Features, VehicleEntity Vehicle)[] Create() =>
    [
        // sahibinden 1351639763 (16 Kasım 2026)
        (
            "2021-audi-q5-40-tdi-quattro-s-line", "Audi", "Q5", "Q5 40 TDI quattro S line", "Mavi",
            [
                "ABS", "ESP", "ASR (Çekiş Kontrolü)", "Sürücü Hava Yastığı", "Yolcu Hava Yastığı", "Yan Hava Yastıkları",
                "Perde Hava Yastıkları", "Isofix", "Lastik Basınç İzleme", "Merkezi Kilit", "Immobilizer",
                "Deri Koltuk", "Elektrikli Ön Koltuklar", "Hafızalı Koltuklar", "Isıtmalı Ön Koltuklar", "Otomatik Klima",
                "Head-up Display", "Dijital Gösterge Paneli", "Fonksiyonel Direksiyon", "Anahtarsız Giriş ve Çalıştırma",
                "Start/Stop", "Elektrikli Ön Camlar", "Elektrikli Arka Camlar", "Ambiyans Aydınlatma", "Elektrikli Bagaj Kapağı",
                "Yol Bilgisayarı", "Geri Görüş Kamerası", "360° Kamera", "Otomatik Park Asistanı",
                "Panoramik Cam Tavan", "LED Far", "Ön Park Sensörü", "Arka Park Sensörü", "Elektrikli Aynalar",
                "Isıtmalı Aynalar", "Alaşım Jant",
                "Bluetooth", "USB / AUX", "Kablosuz Şarj", "Premium Ses Sistemi", "Dokunmatik Ekran"
            ],
            new VehicleEntity
            {
                Id = new Guid("58a449e1-3ebe-47d3-a63d-347c70cd1618"),
                Title = "AUDİ Q5 BANG&OLUF+HAFIZA+ISIT+360KAM+OTOPARK+KEYLES+RECARO",
                Year = 2021,
                Kilometer = 6_500,
                // İlanda "Dizel (Hafif Hibrit)": hafif hibrit ayrı yakıt tipi değil. Çekiş ilanda "4x4", quattro ultra elektronik.
                FuelType = FuelType.Diesel,
                Transmission = TransmissionType.Automatic,
                BodyType = BodyType.Suv,
                EngineCc = 1968,
                PowerHp = 204,
                Drivetrain = Drivetrain.AllWheel,
                InteriorColor = "Siyah Deri",
                Condition = VehicleCondition.Used,
                HasWarranty = true,
                HeavyDamageRecord = false,
                PlateOrigin = PlateOrigin.Turkish,
                AcceptsTrade = true,
                // Kampanya: ilandaki fiyat (5.475.000) kampanyalı fiyattır; sitede normal fiyat üstü çizili görünür.
                Price = 5_650_000m,
                CampaignPrice = 5_475_000m,
                PublishedAt = new DateTime(2026, 11, 16),
                Description = string.Concat(
                    Heading("2021"),
                    Heading("AUDİ Q5 40 TDI QUATTRO S LİNE 4X4"),
                    Line("'SIFIRDAN FARKSIZ'", "red"),
                    Line("\"KM yok denecek kadar az !\"", "red"),
                    Blank,
                    Line("PERİYODİK BAKIMLARI", "blue"),
                    Line("KONYA ATTARLAR YETKİLİ SERVİSTE", "blue"),
                    Line("YENİ(20.09.2026) YAPILMIŞTIR.", "blue"),
                    Blank,
                    Line("AUDİ ZATEN ARABANIN BABASIDIR ;AMA BU ARABA ÖZELLİKLE EMSALİ BULUNMAYACAK, YENİLERLE KIYASLANMAYACAK,", "blue"),
                    Line("SINIF, KOLEKSİYONLUK BİR ARAÇTIR.", "blue"),
                    Blank,
                    Line("FULL + FUL DONANIMA SAHİPTİR.", "blue"),
                    Blank,
                    Line("MUAYENESİ YENİ (EYLÜL-2026) YAPILMIŞTIR.", "blue"),
                    Blank,
                    Line("MASRAFSIZ, SORUNSUZ ALANIN SADECE YAKITINI KOYUP BİNECEĞİ BİR ARABA..", "blue"),
                    Blank,
                    Line("FATURA DEĞERİ YÜKSEKTİR.", "blue"),
                    Blank,
                    Line("Detaylar için arayınız..", "red"),
                    Heading("DONANIM LİSTESİ", "red"),
                    Line("S LİNE LANSMAN MAVİ"),
                    Line("360 DERECE DÖRT BÖLGE KAMERA"),
                    Line("BANG OLUFSEN SES SİSTEMİ"),
                    Line("SPOR DİREKSİYON"),
                    Line("RECARO SPOR KOLTUK"),
                    Line("HAFIZALI ISITMALI DERİ KOLTUKLAR"),
                    Line("KABLOSUZ TELEFON ŞARJI"),
                    Line("BARDAK ISITMA-SOĞUTMALARI"),
                    Line("HAYALET EKRAN"),
                    Line("ANAHTARSIZ GİRİŞ ÇIKIŞ"),
                    Line("ANAHTARSIZ ÇALIŞTIRMA"),
                    Line("ELEKTRİKLİ BAGAJ KAPAĞI"),
                    Line("S LİNE JANTLAR"),
                    Blank,
                    Signature)
            }
        ),
        // sahibinden 1351620416 (16 Kasım 2026)
        (
            "2023-mercedes-benz-g-400-d", "Mercedes-Benz", "G Serisi", "G 400 d", "Siyah",
            [
                "ABS", "ESP", "ASR (Çekiş Kontrolü)", "Sürücü Hava Yastığı", "Yolcu Hava Yastığı", "Yan Hava Yastıkları",
                "Perde Hava Yastıkları", "Isofix", "Lastik Basınç İzleme", "Merkezi Kilit", "Immobilizer",
                "Deri Koltuk", "Elektrikli Ön Koltuklar", "Hafızalı Koltuklar", "Isıtmalı Ön Koltuklar", "Soğutmalı Koltuklar",
                "Masajlı Koltuklar", "Otomatik Klima", "Dijital Gösterge Paneli", "Fonksiyonel Direksiyon", "Start/Stop",
                "Elektrikli Ön Camlar", "Elektrikli Arka Camlar", "Ambiyans Aydınlatma", "Yol Bilgisayarı",
                "Geri Görüş Kamerası", "360° Kamera",
                "Sunroof", "LED Far", "Ön Park Sensörü", "Arka Park Sensörü", "Elektrikli Aynalar", "Isıtmalı Aynalar",
                "Alaşım Jant", "Karartılmış Arka Camlar",
                "Bluetooth", "USB / AUX", "Navigasyon", "Premium Ses Sistemi"
            ],
            new VehicleEntity
            {
                Id = new Guid("f6e9652c-158b-4064-8ee8-4d62c1b08742"),
                Title = "FATİH TAŞCI'DAN 2023 G400D MANUFAKTUR TAS. MASAJ-ISIT-SOĞUT-360",
                Year = 2023,
                Kilometer = 88_000,
                FuelType = FuelType.Diesel,
                Transmission = TransmissionType.Automatic,
                BodyType = BodyType.Suv,
                EngineCc = 2925,
                PowerHp = 330,
                // İlanda "4x4": G Serisi'nde üç diferansiyel kilitli sürekli dört çeker.
                Drivetrain = Drivetrain.FourWheel,
                InteriorColor = "Bengal Kırmızısı Nappa Deri",
                Condition = VehicleCondition.Used,
                HasWarranty = true,
                HeavyDamageRecord = false,
                PlateOrigin = PlateOrigin.Turkish,
                AcceptsTrade = true,
                // Açıklamada "hata, boya, değişen, tramer yoktur": parçalar orijinal, TramerAmount boş (kayıt yok).
                Price = 15_950_000m,
                PublishedAt = new DateTime(2026, 11, 16),
                Description = string.Concat(
                    Heading("2023 MODEL"),
                    Heading("MERCEDES-BENZ G400 D 2024 BAYİ ÇIKIŞLI"),
                    Line("ÖZEL MANUFAKTUR SCHÖCKL TASARIM"),
                    Line("METALİK OBSİDİYEN SİYAHI"),
                    Line("MANUFAKTUR BENGAL KIRMIZISI NAPPA DERİ İÇ TASARIM"),
                    Line("HATA BOYA DEĞİŞEN TRAMER YOKTUR.", "blue"),
                    Line("TİTİZLİKLE KULLANILMIŞ", "blue"),
                    Line("TÜM BAKIMLARI YETKİLİ SERVİSTE EKSİKSİZ YAPILMIŞTIR.", "blue"),
                    Line("GARANTİSİ DEVAM ETMEKTEDİR.", "blue"),
                    Line("4 ADET LASTİK SIFIR TAKILMIŞTIR.", "blue"),
                    Line("MUAYENESİ YENİDİR.", "blue"),
                    Blank,
                    Heading("DONANIM LİSTESİ", "red"),
                    Line("MANUFAKTUR SCHÖCKL TASARIM PAKETİ"),
                    Line("AMG İÇ VE DIŞ TASARIM (603.648 ₺)"),
                    Line("MANUFAKTUR BENGAL KIRMIZISI NAPPA DERİ İÇ TASARIM (184.320 ₺)"),
                    Line("MANUFAKTUR GECE PAKETİ OBSİDYEN SİYAHI TAMPON, RADYATÖR IZGARASI, TEKERLEK ÇAMURLUĞU VE JANTLAR, (718.848₺)"),
                    Line("MANUFAKTUR OBSİDYEN SİYAHI BOYALI TAVAN (380.160₺)"),
                    Line("AKTİF ERGONOMİK KOLTUK PAKETİ PLUS İÇ MEKAN HAVA İYONİZASYONU (747.648 ₺)"),
                    Line("MANUFAKTUR AÇIK GÖZENEKLİ GRİ MEŞE AHŞAP SÜSLEMELER(138.240₺)"),
                    Line("MANUFAKTUR OBSİDİYEN SİYAHI STEPNE KAPAĞI HALKASI (248.832₺)"),
                    Line("DASHCAM (ARAÇ İÇİ KAMERA) (46.080 ₺)"),
                    Line("MANUKFAKTUR YAN BASAMAKLAR (161.280 ₺)"),
                    Line("20'' 10 KOLLU AMG TASARIMI MULTİ-SPOKE PARLAK SİYAH ALAŞIM JANTLAR (46.080 ₺)"),
                    Line("Burmester® SURROUND SES SİSTEMİ"),
                    Line("PARMAK İZİ OKUYUCU"),
                    Line("MBUX NAVİGASYON PREMİUM"),
                    Note("(CANLI TRAFİK BİLGİLERİ, 3 BOYUTLU HARİTALAMA, YAPAY ZEKA DESTEKLİ ROTA TAHMİNLERİ)"),
                    Line("MBUX SANAL GERÇEKLİĞE SAHİP NAVİGASYON"),
                    Note("(ÖN KAMERANIN KAYDETTİĞİ CANLI YOL GÖRÜNTÜSÜ ÜZERİNE YÖNLENDİRME OKLARI, SOKAK İSİMLERİ VE EV NUMARALARI GİBİ GRAFİKLERİN CANLI OLARAK EKLENMESİNİ SAĞLAYAN SİSTEM)"),
                    Line("100 % DİFERANSİYEL KİLİTLERİ"),
                    Note("(SEÇİLEBİLİR 3 DİFERANSİYEL KİLİDİ, %100 KİLİTLEME ETKİSİYLE EN ZORLU ARAZİ KOŞULLARINDA DAHİ MAKSİMUM İTİŞ VE ÇEKİŞ GÜCÜ SAĞLAR.)"),
                    Line("MASAJ FONKSİYONLU KOLTUKLAR"),
                    Line("360 DERECE 5 BÖLGE KAMERA"),
                    Line("YENİ NESİL MB ACTIVE MULTİMEDİA EKRAN (GENİŞ EKRAN)"),
                    Line("KOLTUK HAFIZA PAKETİ"),
                    Line("KOLTUK ISITMA PAKETİ"),
                    Line("KOLTUK SOĞUTMA PAKETİ"),
                    Line("AKTİF NAVİGASYON PAKETİ"),
                    Line("GERÇEK DERİ TORPİDO"),
                    Line("ISITMALI ÖN CAM"),
                    Line("ÖN KOLTUKLAR İÇİN"),
                    Line("MERCEDES-BENZ YAZAN FREN KALİPERİ"),
                    Line("DINAMICA MICROFİBER SİYAH TAVAN DÖŞEMESİ"),
                    Line("Hızlanma (0-100 km/sa) 5,8 s"),
                    Blank,
                    Signature)
            }
        ),
        // sahibinden 1351656971 (16 Kasım 2026)
        (
            "2024-ford-puma-1-0-ecoboost-style", "Ford", "Puma", "Puma 1.0 EcoBoost Style", "Gri",
            [
                "ABS", "ESP", "ASR (Çekiş Kontrolü)", "Sürücü Hava Yastığı", "Yolcu Hava Yastığı", "Yan Hava Yastıkları",
                "Perde Hava Yastıkları", "Isofix", "Kör Nokta Uyarısı", "Şerit Takip", "Lastik Basınç İzleme", "Merkezi Kilit",
                "Immobilizer",
                "Kumaş Koltuk", "Otomatik Klima", "Adaptif Hız Sabitleyici", "Fonksiyonel Direksiyon", "Start/Stop",
                "Elektrikli Ön Camlar", "Elektrikli Arka Camlar", "Yol Bilgisayarı",
                "Yağmur Sensörü", "Arka Park Sensörü", "Elektrikli Aynalar", "Isıtmalı Aynalar", "Alaşım Jant",
                "Bluetooth", "USB / AUX", "Apple CarPlay", "Android Auto", "Dokunmatik Ekran"
            ],
            new VehicleEntity
            {
                Id = new Guid("bcdb08af-cce6-4e13-8b03-3d69e2da7d30"),
                Title = "FATİH TAŞCI'DAN 2024 FORD PUMA HATASIZ BOYASIZ OTOMATİK VİTES",
                Year = 2024,
                Kilometer = 45_000,
                FuelType = FuelType.Gasoline,
                Transmission = TransmissionType.Automatic,
                BodyType = BodyType.Suv,
                EngineCc = 999,
                PowerHp = 125,
                Drivetrain = Drivetrain.FrontWheel,
                InteriorColor = "Siyah Kumaş",
                Condition = VehicleCondition.Used,
                HasWarranty = true,
                HeavyDamageRecord = false,
                PlateOrigin = PlateOrigin.Turkish,
                AcceptsTrade = true,
                // Açıklamada "değişensiz, boyasız, tramersiz": parçalar orijinal, TramerAmount boş (kayıt yok).
                // Kampanya: ilandaki fiyat (1.475.000) kampanyalı fiyattır; sitede normal fiyat üstü çizili görünür.
                Price = 1_525_000m,
                CampaignPrice = 1_475_000m,
                PublishedAt = new DateTime(2026, 11, 16),
                Description = string.Concat(
                    Heading("2024 FORD PUMA 1.0 TSI STYLE"),
                    Line("DEĞİŞENSİZ, BOYASIZ, TRAMERSİZ", "blue"),
                    Line("İLK SAHİBİNDEN İÇİ DIŞI TERTEMİZ, TİTİZLİKLE KULLANILMIŞ BİR ARABADIR.", "blue"),
                    Line("YETKİLİ SERVİS BAKIMLI", "blue"),
                    Line("AKÜ VE BALATALAR YENİ DEĞİŞMİŞTİR.", "blue"),
                    Line("4 ADET SIFIR LASTİK YENİ TAKILMIŞTIR.", "blue"),
                    Heading("--- DONANIM ---", "red"),
                    Line("START STOP"),
                    Line("17' İNÇ JANTLAR"),
                    Line("CAR PLAY"),
                    Line("ŞERİT TAKİP ASİSTANI"),
                    Line("KÖR NOKTA ASİSTANI"),
                    Line("ADAPTİF HIZ SABİTLEME"),
                    Line("GERİ PARK SENSÖRÜ"),
                    Line("DİJİTAL KLİMA"),
                    Line("S.O.S ACİL DURUM SİSTEMİ"),
                    Line("DOKUNMATİK MULTİMEDYA EKRAN"),
                    Line("BLUETOOH TELEFON"),
                    Line("ISITMALI AYNA"),
                    Line("ELEKTRİKLİ AYNALAR"),
                    Line("YAĞMUR SENSÖRÜ"))
            }
        ),
        // sahibinden 1351649764 (16 Kasım 2026)
        (
            "2023-mercedes-benz-e-200-d-exclusive", "Mercedes-Benz", "E Serisi", "E 200 d Exclusive", "Gri",
            [
                "ABS", "ESP", "ASR (Çekiş Kontrolü)", "Sürücü Hava Yastığı", "Yolcu Hava Yastığı", "Yan Hava Yastıkları",
                "Perde Hava Yastıkları", "Isofix", "Kör Nokta Uyarısı", "Şerit Takip", "Ön Çarpışma Uyarısı",
                "Acil Fren Asistanı", "Yorgunluk Tespiti", "Yokuş Kalkış Desteği", "Lastik Basınç İzleme", "Merkezi Kilit",
                "Immobilizer",
                "Deri Koltuk", "Elektrikli Ön Koltuklar", "Hafızalı Koltuklar", "Isıtmalı Ön Koltuklar", "Otomatik Klima",
                "Çift Bölgeli Klima", "Hız Sabitleyici", "Dijital Gösterge Paneli", "Fonksiyonel Direksiyon",
                "Anahtarsız Giriş ve Çalıştırma", "Start/Stop", "Elektrikli Ön Camlar", "Elektrikli Arka Camlar",
                "Ambiyans Aydınlatma", "Elektrikli Bagaj Kapağı", "Yol Bilgisayarı", "Geri Görüş Kamerası",
                "Otomatik Park Asistanı",
                "Panoramik Cam Tavan", "LED Far", "Ön Park Sensörü", "Arka Park Sensörü", "Elektrikli Aynalar",
                "Isıtmalı Aynalar", "Elektrikli Katlanır Aynalar", "Otomatik Kararan Ayna", "Alaşım Jant",
                "Bluetooth", "USB / AUX", "Apple CarPlay", "Android Auto", "Premium Ses Sistemi", "Dokunmatik Ekran",
                "Sesli Komut"
            ],
            new VehicleEntity
            {
                Id = new Guid("455fae2f-9fe5-466b-a3c7-45b650f2ff77"),
                Title = "FATİH TAŞCI'DAN BURMESTER-Ç.HAFIZA-VAKUM-E.BAGAJ",
                Year = 2023,
                Kilometer = 109_000,
                FuelType = FuelType.Diesel,
                Transmission = TransmissionType.Automatic,
                BodyType = BodyType.Sedan,
                EngineCc = 1597,
                PowerHp = 160,
                Drivetrain = Drivetrain.RearWheel,
                InteriorColor = "Fındık Kahvesi Artico Deri",
                Condition = VehicleCondition.Used,
                HasWarranty = true,
                HeavyDamageRecord = false,
                PlateOrigin = PlateOrigin.Turkish,
                AcceptsTrade = true,
                // Açıklamada "hatasız, boyasız, değişensiz" ve "39.000 tramer plastik aksam": parçalar orijinal.
                TramerAmount = 39_000m,
                Price = 4_675_000m,
                PublishedAt = new DateTime(2026, 11, 16),
                Description = string.Concat(
                    Heading("2023 MERCEDES E 200 D EXCLUSİVE"),
                    Line("YETKİLİ SERVİS BAKIMLI", "blue"),
                    Line("FINDIK KAHVESİ ARTICO TAM DERİ / ESPRESSO KAHVESİ KONSOL", "blue"),
                    Line("HATASIZ, BOYASIZ, DEĞİŞENSİZ", "blue"),
                    Line("39.000 TRAMER PLASTİK AKSAN", "blue"),
                    Heading("----- Donanım Listesi -----", "red"),
                    Line("BURMESTER SES SİSTEMİ", "red"),
                    Line("ÇİFT BÖLGE ELEKTRİKLİ HAFIZALI ÖN KOLTUKLAR", "red"),
                    Line("VAKUMLU KAPILAR", "red"),
                    Line("KOLTUK ISITMALARI", "red"),
                    Line("ELEKTRİKLİ ARKA PERDE", "red"),
                    Line("ELEKTRİKLİ BAGAJ KAPAĞI", "red"),
                    Line("64 RENKLİ AMBİYANS AYDINLATMASI", "red"),
                    Line("ANAHTARSIZ ÇALIŞTIRMA", "red"),
                    Line("ARKA DİJİTAL KLİMA", "red"),
                    Line("KAHVERENGİ DİŞBUDAK AĞACINDAN MAT AHŞAP KAPLAMALAR"),
                    Line("ÇARPIŞMA ÖNLEME VE YAYA KORUMA"),
                    Line("OTOMATİK PARK ASİSTANI"),
                    Line("ELEKTRİKLİ ISITMALI OTOMATİK HAFIZALI KATLANABİLİR YAN AYNALAR"),
                    Line("KÖR NOKTA ASİSTANI"),
                    Line("ŞERİT TAKİP ASİSTANI"),
                    Line("YORGUNLUK ALGILAMA SİSTEMİ"),
                    Line("AUTO HOLD YOKUŞ KALKIŞ DESTEĞİ"),
                    Line("GERİ GÖRÜŞ KAMERASI"),
                    Line("TORPİDO SOĞUTMA"),
                    Line("HIZ SABİTLEME"),
                    Line("APPLE CAR PLAY"),
                    Line("ANDROİD AUTO"),
                    Blank,
                    Signature)
            }
        )
    ];

    // Açıklama editörünün biçimi (rich-text-format.ts): hiza ve renk sınıfla, satırlar arasında boşluk yok (vitrinde
    // pre-line). Seeder açıklamayı panel kaydıyla aynı temizlikten geçirir (VehicleService.SanitizeDescription).

    private const string Blank = "<p></p>";

    private const string Signature = "<p class=\"ta-center\"><strong><em>İŞİMİZ MEMNUNİYETİNİZ..</em></strong></p>";

    private static string Heading(string text, string color = null) =>
        $"<h3 class=\"ta-center\">{Colored(text, color)}</h3>";

    private static string Line(string text, string color = null) =>
        $"<p class=\"ta-center\"><strong>{Colored(text, color)}</strong></p>";

    /// <summary>
    /// Bir üst satırın küçük punto açıklaması (sahibinden'de küçük yazı): ortalı, kalın değil.
    /// </summary>
    private static string Note(string text) =>
        $"<p class=\"ta-center\">{text}</p>";

    private static string Colored(string text, string color) =>
        color == null ? text : $"<span class=\"tc-{color}\">{text}</span>";
}
