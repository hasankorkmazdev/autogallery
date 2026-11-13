using FatihTasci.Core.Enums;
using System.Globalization;

namespace FatihTasci.Server.Helpers;

/// <summary>
/// Vitrin sayfalarında sayı, fiyat ve bağlantı biçimlendirmesi.
/// </summary>
public static class SiteFormat
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    public static string Price(decimal? price, bool showPrice)
    {
        if (!showPrice || !price.HasValue || price <= 0)
            return "Fiyat için arayınız";

        return $"{price.Value.ToString("N0", Turkish)} TL";
    }

    public static string Number(int? value) => value?.ToString("N0", Turkish) ?? "-";

    public static string Km(int? value) => value.HasValue ? $"{Number(value)} km" : "-";

    public static string StatusBadge(VehicleStatus status) => status switch
    {
        VehicleStatus.Reserved => "Rezerve",
        VehicleStatus.Sold => "Satıldı",
        _ => null
    };

    /// <summary>
    /// wa.me bağlantısı (ilan no ile hazır mesaj)
    /// </summary>
    public static string WhatsAppLink(string number, string message)
    {
        if (string.IsNullOrWhiteSpace(number))
            return null;

        return $"https://wa.me/{number}?text={Uri.EscapeDataString(message ?? string.Empty)}";
    }

    public static string TelLink(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var digits = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
        return $"tel:{digits}";
    }
}
