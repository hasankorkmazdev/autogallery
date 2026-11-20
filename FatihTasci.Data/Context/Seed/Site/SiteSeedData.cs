using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Setting;
using System.Text.Json;

namespace FatihTasci.Data.Context.Seed.Site;

/// <summary>
/// Site ayarları ve showroom sahneleri (SiteSeeder).
/// </summary>
internal static class SiteSeedData
{
    /// <summary>
    /// Ayar anahtarı ve değeri; yalnız eksik anahtar eklenir. Yerelden dışa aktarılan site verisi (Assets/Seed/Site/site.json)
    /// bunlardan önce gelir: ikisinde de olan anahtar site.json'daki değerle eklenir.
    /// </summary>
    public static readonly (string Key, string Value)[] Settings =
    [
        (nameof(SiteSettingsModel.GalleryName), "Fatih Taşcı Otomotiv"),
        // Galerinin sahibinden mağazasındaki numaralar: iş telefonu ve cep (WhatsApp, ülke koduyla yalnız rakam).
        (nameof(SiteSettingsModel.Phone), "0 (332) 342 04 13"),
        (nameof(SiteSettingsModel.WhatsApp), "905324411945"),
        // Sosyal medya: SiteSettingService'in sakladığı biçimde (SocialLinkModel listesinin JSON'u).
        (nameof(SiteSettingsModel.SocialLinks), JsonSerializer.Serialize(new List<SocialLinkModel>
        {
            new() { Platform = SocialPlatform.Instagram, Url = "https://www.instagram.com/fatihhtassci/" }
        })),
        (nameof(SiteSettingsModel.Address), "Fevzi Çakmak Mahallesi, 10437. Sokak, Karatay / Konya"),
        (nameof(SiteSettingsModel.Latitude), "37.908418"),
        (nameof(SiteSettingsModel.Longitude), "32.517162"),
        (nameof(SiteSettingsModel.WorkingHours), "Pazartesi - Cumartesi 09:00 - 19:00")
    ];

    /// <summary>
    /// Galeri atmosferinin sahneleri (Id, etiket, başlık); sıra listedeki sıradır. Fotoğraf panelden yüklenir ya da
    /// site.json'dan gelir; fotoğrafsız sahne vitrinde görünmez.
    /// </summary>
    public static readonly (int Id, string Tag, string Title)[] ShowroomScenes =
    [
        (1, "Dış cephe & vitrin", "Showroom ve Vitrin"),
        (2, "Sergi salonu", "Işıklandırılmış Sergi Salonu"),
        (3, "Misafir alanı", "Kahve ve Birebir Görüşme"),
        (4, "Hazırlık", "Detaylı Bakım ve Hazırlık"),
        (5, "Ekspertiz", "Kontrol ve Ekspertiz Alanı"),
        (6, "Teslimat", "Teslimat Sahnesi")
    ];
}
