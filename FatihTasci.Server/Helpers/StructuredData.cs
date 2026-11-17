using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Public;
using FatihTasci.Core.Models.Domain.Setting;
using Microsoft.AspNetCore.Html;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FatihTasci.Server.Helpers;

/// <summary>
/// schema.org JSON-LD blokları: galeri (AutoDealer), araç (Car + Offer), sayfa yolu (BreadcrumbList).
/// JSON varsayılan kodlayıcıyla yazılır; &lt; &gt; &amp; kaçırıldığı için script bloğundan taşamaz.
/// CSP script-src bu blokları etkilemez (tarayıcı çalıştırmaz, yalnız veri).
/// </summary>
public static class StructuredData
{
    private const string Context = "https://schema.org";

    public static IHtmlContent Script(object data)
    {
        return new HtmlString($"<script type=\"application/ld+json\">{JsonSerializer.Serialize(data)}</script>");
    }

    public static string DealerId(string baseUrl) => $"{baseUrl}/#galeri";

    public static Dictionary<string, object> Dealer(SiteSettingsModel settings, string baseUrl, string imageUrl)
    {
        var dealer = Node("AutoDealer",
            ("@id", DealerId(baseUrl)),
            ("name", SiteFormat.GalleryName(settings)),
            ("url", baseUrl + "/"),
            ("logo", settings.LogoUrl == null ? null : baseUrl + settings.LogoUrl),
            ("image", imageUrl),
            ("telephone", settings.Phone),
            ("email", settings.Email),
            ("address", string.IsNullOrWhiteSpace(settings.Address) ? null : Node("PostalAddress",
                ("streetAddress", settings.Address.Trim()),
                ("addressCountry", "TR"))),
            ("hasMap", MapLinks.GoogleMaps(settings)),
            ("sameAs", settings.SocialLinks.Select(x => x.Url).Distinct().ToArray()));

        if (MapLinks.TryGetCoordinates(settings, out var lat, out var lng))
            dealer["geo"] = Node("GeoCoordinates", ("latitude", lat), ("longitude", lng));

        return WithContext(dealer);
    }

    public static Dictionary<string, object> Vehicle(PublicVehicleDetailModel model, SiteSettingsModel settings, string baseUrl)
    {
        var url = baseUrl + model.Url;
        var condition = model.Condition == VehicleCondition.New ? "https://schema.org/NewCondition" : "https://schema.org/UsedCondition";
        var showPrice = model.ShowPrice && model.Status != VehicleStatus.Sold;
        var price = SiteFormat.HasCampaign(model.Price, model.CampaignPrice, model.ShowPrice) ? model.CampaignPrice : model.Price;

        var offer = Node("Offer",
            ("url", url),
            ("priceCurrency", "TRY"),
            ("price", showPrice && price > 0 ? price.Value.ToString("0.##", CultureInfo.InvariantCulture) : null),
            ("availability", model.Status switch
            {
                VehicleStatus.Sold => "https://schema.org/SoldOut",
                VehicleStatus.Reserved => "https://schema.org/Reserved",
                _ => "https://schema.org/InStock"
            }),
            ("itemCondition", condition),
            ("seller", Node("AutoDealer",
                ("@id", DealerId(baseUrl)),
                ("name", SiteFormat.GalleryName(settings)),
                ("telephone", string.IsNullOrWhiteSpace(model.ContactPhone) ? settings.Phone : model.ContactPhone))));

        var car = Node("Car",
            ("name", model.Title),
            ("url", url),
            ("sku", model.ListingNo.ToString(CultureInfo.InvariantCulture)),
            ("image", model.Images.Take(10).Select(x => baseUrl + x.LargeUrl).ToArray()),
            ("description", PlainText(model.Description) ?? model.Title),
            ("brand", Node("Brand", ("name", model.BrandName))),
            ("model", model.ModelName),
            ("vehicleModelDate", model.Year?.ToString(CultureInfo.InvariantCulture)),
            ("mileageFromOdometer", model.Kilometer.HasValue ? Node("QuantitativeValue", ("value", model.Kilometer.Value), ("unitCode", "KMT")) : null),
            ("fuelType", model.FuelName),
            ("vehicleTransmission", model.TransmissionName),
            ("bodyType", model.BodyTypeName),
            ("driveWheelConfiguration", DriveWheel(model.Drivetrain)),
            ("vehicleEngine", model.EngineCc.HasValue || model.PowerHp.HasValue
                ? Node("EngineSpecification",
                    ("engineDisplacement", model.EngineCc.HasValue ? Node("QuantitativeValue", ("value", model.EngineCc.Value), ("unitCode", "CMQ")) : null),
                    ("enginePower", model.PowerHp.HasValue ? Node("QuantitativeValue", ("value", model.PowerHp.Value), ("unitCode", "BHP")) : null))
                : null),
            ("color", model.ExteriorColorName),
            ("vehicleInteriorColor", model.InteriorColor),
            ("itemCondition", condition),
            ("offers", offer));

        return WithContext(car);
    }

    /// <summary>
    /// Sayfa yolu. Son öğe bulunulan sayfadır.
    /// </summary>
    public static Dictionary<string, object> Breadcrumb(string baseUrl, params (string Name, string Path)[] items)
    {
        var list = Node("BreadcrumbList",
            ("itemListElement", items.Select((item, index) => Node("ListItem",
                ("position", index + 1),
                ("name", item.Name),
                ("item", baseUrl + item.Path))).ToArray()));
        return WithContext(list);
    }

    private static string DriveWheel(Drivetrain? drivetrain) => drivetrain switch
    {
        Drivetrain.FrontWheel => "https://schema.org/FrontWheelDriveConfiguration",
        Drivetrain.RearWheel => "https://schema.org/RearWheelDriveConfiguration",
        Drivetrain.FourWheel => "https://schema.org/FourWheelDriveConfiguration",
        Drivetrain.AllWheel => "https://schema.org/AllWheelDriveConfiguration",
        _ => null
    };

    /// <summary>
    /// Temizlenmiş HTML açıklamayı düz metne çevirir (en çok 5000 karakter).
    /// </summary>
    private static string PlainText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var text = WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]+>", " "));
        text = Regex.Replace(text, @"\s+", " ").Trim();
        if (text.Length == 0)
            return null;
        return text.Length <= 5000 ? text : text[..4999] + "…";
    }

    private static Dictionary<string, object> WithContext(Dictionary<string, object> node)
    {
        var root = new Dictionary<string, object> { ["@context"] = Context };
        foreach (var pair in node)
            root[pair.Key] = pair.Value;
        return root;
    }

    /// <summary>
    /// Boş değerleri (null, boş metin, boş dizi) atlayan JSON-LD düğümü
    /// </summary>
    private static Dictionary<string, object> Node(string type, params (string Key, object Value)[] properties)
    {
        var node = new Dictionary<string, object> { ["@type"] = type };
        foreach (var (key, value) in properties)
        {
            if (value == null || value is string s && string.IsNullOrWhiteSpace(s) || value is Array { Length: 0 })
                continue;
            node[key] = value;
        }
        return node;
    }
}
