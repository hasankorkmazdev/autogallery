using FatihTasci.Core.Enums;
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
