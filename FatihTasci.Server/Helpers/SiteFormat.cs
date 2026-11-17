using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Setting;
using Microsoft.AspNetCore.Html;
using System.Globalization;
using System.Text.Encodings.Web;

namespace FatihTasci.Server.Helpers;

/// <summary>
/// Vitrin sayfalarında sayı, fiyat ve bağlantı biçimlendirmesi.
/// </summary>
public static class SiteFormat
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    public static string GalleryName(SiteSettingsModel settings)
    {
        return string.IsNullOrWhiteSpace(settings.GalleryName) ? "Oto Galeri" : settings.GalleryName.Trim();
    }

    /// <summary>
    /// Meta açıklaması için: en çok maxLength karakter, kelime ortasında kesmez.
    /// </summary>
    public static string Truncate(string text, int maxLength = 160)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var value = string.Join(' ', text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
        if (value.Length <= maxLength)
            return value;

        var cut = value.LastIndexOf(' ', maxLength - 1);
        return value[..(cut > maxLength / 2 ? cut : maxLength - 1)].TrimEnd(' ', ',', '.', '·', '—') + "…";
    }

    public static string Price(decimal? price, bool showPrice)
    {
        if (!showPrice || !price.HasValue || price <= 0)
            return "Fiyat için arayınız";

        return Money(price.Value);
    }

    public static string Money(decimal value) => $"{value.ToString("N0", Turkish)} TL";

    /// <summary>
    /// Fiyat gösteriliyor ve kampanyalı fiyat normal fiyattan düşükse kampanya vardır.
    /// </summary>
    public static bool HasCampaign(decimal? price, decimal? campaignPrice, bool showPrice)
    {
        return showPrice && price > 0 && campaignPrice > 0 && campaignPrice < price;
    }

    /// <summary>
    /// Ekranda geçerli olan fiyat metni (kampanya varsa kampanyalı fiyat).
    /// </summary>
    public static string EffectivePrice(decimal? price, decimal? campaignPrice, bool showPrice)
    {
        return HasCampaign(price, campaignPrice, showPrice) ? Money(campaignPrice.Value) : Price(price, showPrice);
    }

    /// <summary>
    /// Kampanya varsa eski fiyat üstü çizili, kampanyalı fiyat vurgulu; yoksa düz fiyat.
    /// İçerik yalnızca biçimlendirilmiş sayılardan oluşur.
    /// </summary>
    public static IHtmlContent PriceTag(decimal? price, decimal? campaignPrice, bool showPrice)
    {
        if (!HasCampaign(price, campaignPrice, showPrice))
            return new HtmlString(HtmlEncoder.Default.Encode(Price(price, showPrice)));

        return new HtmlString(
            $"<del class=\"price-old\"><span class=\"sr-only\">Eski fiyat: </span>{Money(price.Value)}</del> " +
            $"<ins class=\"price-new\"><span class=\"sr-only\">Kampanyalı fiyat: </span>{Money(campaignPrice.Value)}</ins>");
    }

    /// <summary>
    /// Fotoğraf varyantları için srcset (480 / 1280 / 2400). Boş varyant atlanır.
    /// </summary>
    public static string SrcSet(string small, string medium, string large)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(small)) parts.Add($"{small} 480w");
        if (!string.IsNullOrEmpty(medium)) parts.Add($"{medium} 1280w");
        if (!string.IsNullOrEmpty(large)) parts.Add($"{large} 2400w");
        return string.Join(", ", parts);
    }

    public static string Number(int? value) => value?.ToString("N0", Turkish) ?? "-";

    public static string Km(int? value) => value.HasValue ? $"{Number(value)} km" : "-";

    /// <summary>
    /// Gün ay yıl, ay adıyla: "17 Kasım 2026"
    /// </summary>
    public static string Date(DateTime value) => value.ToString("d MMMM yyyy", Turkish);

    /// <summary>
    /// Ay yıl: "Kasım 2026"
    /// </summary>
    public static string MonthYear(DateTime value) => value.ToString("MMMM yyyy", Turkish);

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

    /// <summary>
    /// Türkiye cep telefonunu WhatsApp numarasına çevirir (0532 123 45 67 -> 905321234567). Cep değilse null.
    /// </summary>
    public static string MobileToWhatsApp(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length == 11 && digits.StartsWith("05"))
            digits = "9" + digits;
        else if (digits.Length == 10 && digits.StartsWith('5'))
            digits = "90" + digits;

        return digits.Length == 12 && digits.StartsWith("905") ? digits : null;
    }

    public static string TelLink(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var digits = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
        return $"tel:{digits}";
    }

    /// <summary>
    /// Panelden girilen düz metni paragraflara böler: boş satırla ayrılan her blok bir &lt;p&gt;,
    /// blok içindeki satır sonu &lt;br&gt; olur. Metin kodlanır, HTML yazılamaz.
    /// </summary>
    public static IHtmlContent Paragraphs(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return HtmlString.Empty;

        var blocks = text.Replace("\r\n", "\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(block => "<p>" + string.Join("<br />", block.Split('\n').Select(line => HtmlEncoder.Default.Encode(line.Trim()))) + "</p>");
        return new HtmlString(string.Concat(blocks));
    }
}
