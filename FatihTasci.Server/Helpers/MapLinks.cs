using FatihTasci.Core.Models.Domain.Setting;
using System.Globalization;

namespace FatihTasci.Server.Helpers;

/// <summary>
/// Galeri konumu için harita ve yol tarifi bağlantıları. Koordinat varsa koordinat, yoksa adres kullanılır.
/// Gömülü harita yalnız ziyaretçi "Haritayı göster"e basınca yüklenir (KVKK; site.js data-map).
/// </summary>
public static class MapLinks
{
    public static bool TryGetCoordinates(SiteSettingsModel settings, out decimal latitude, out decimal longitude)
    {
        longitude = 0;
        return decimal.TryParse(settings.Latitude, NumberStyles.Number, CultureInfo.InvariantCulture, out latitude)
            && decimal.TryParse(settings.Longitude, NumberStyles.Number, CultureInfo.InvariantCulture, out longitude)
            && latitude is >= -90 and <= 90 && longitude is >= -180 and <= 180;
    }

    public static bool HasLocation(SiteSettingsModel settings)
    {
        return TryGetCoordinates(settings, out _, out _) || !string.IsNullOrWhiteSpace(settings.Address);
    }

    private static string Query(SiteSettingsModel settings)
    {
        if (TryGetCoordinates(settings, out var lat, out var lng))
            return $"{lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)}";

        return string.IsNullOrWhiteSpace(settings.Address) ? null : settings.Address.Trim();
    }

    public static string GoogleMaps(SiteSettingsModel settings)
    {
        var query = Query(settings);
        return query == null ? null : $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(query)}";
    }

    public static string Directions(SiteSettingsModel settings)
    {
        var query = Query(settings);
        return query == null ? null : $"https://www.google.com/maps/dir/?api=1&destination={Uri.EscapeDataString(query)}";
    }

    public static string AppleMaps(SiteSettingsModel settings)
    {
        var query = Query(settings);
        return query == null ? null : $"https://maps.apple.com/?daddr={Uri.EscapeDataString(query)}";
    }

    public static string Yandex(SiteSettingsModel settings)
    {
        if (TryGetCoordinates(settings, out var lat, out var lng))
            return $"https://yandex.com.tr/harita/?rtext=~{lat.ToString(CultureInfo.InvariantCulture)}%2C{lng.ToString(CultureInfo.InvariantCulture)}&rtt=auto";

        return string.IsNullOrWhiteSpace(settings.Address) ? null : $"https://yandex.com.tr/harita/?text={Uri.EscapeDataString(settings.Address.Trim())}";
    }

    /// <summary>
    /// Google Haritalar gömme adresi (API anahtarı gerekmez). CSP frame-src'de https://www.google.com izinlidir.
    /// </summary>
    public static string Embed(SiteSettingsModel settings)
    {
        var query = Query(settings);
        return query == null ? null : $"https://www.google.com/maps?q={Uri.EscapeDataString(query)}&z=16&hl=tr&output=embed";
    }
}
