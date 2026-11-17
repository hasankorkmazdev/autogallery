using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Content;
using FatihTasci.Core.Service.Domains.Abstracts;
using FatihTasci.Server.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace FatihTasci.Server.Controllers.Site;

/// <summary>
/// robots.txt ve sitemap.xml. Adresler Site:BaseUrl'den (boşsa isteğin adresinden) üretilir.
/// </summary>
public class SeoController : Controller
{
    private static readonly XNamespace SitemapNs = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private static readonly XNamespace ImageNs = "http://www.google.com/schemas/sitemap-image/1.1";

    /// <summary>
    /// Sabit sayfalar (ilan sayfaları ve araçlar servisten gelir). Hizmet sayfalarının adresi hizmet türünden üretilir.
    /// </summary>
    private static readonly string[] StaticPaths = new[] { "/", "/ilanlar", "/hizmetler" }
        .Concat(Enum.GetValues<SiteServiceType>().Select(type => new ServiceContentModel { Type = type }.Path))
        .Concat(new[] { "/kurumsal", "/iletisim", "/kvkk" })
        .ToArray();

    private readonly IPublicVehicleService _publicVehicleService;
    private readonly IDeliveryService _deliveryService;
    private readonly SiteUrlBuilder _urls;

    public SeoController(IPublicVehicleService publicVehicleService, IDeliveryService deliveryService, SiteUrlBuilder urls)
    {
        _publicVehicleService = publicVehicleService;
        _deliveryService = deliveryService;
        _urls = urls;
    }

    [HttpGet("robots.txt")]
    [ResponseCache(Duration = 3600)]
    public IActionResult Robots()
    {
        var text = new StringBuilder()
            .AppendLine("User-agent: *")
            .AppendLine("Disallow: /panel/")
            .AppendLine("Disallow: /api/")
            .AppendLine("Disallow: /talep")
            .AppendLine("Disallow: /hata/")
            .AppendLine()
            .AppendLine($"Sitemap: {_urls.Absolute(Request, "/sitemap.xml")}")
            .ToString();
        return Content(text, "text/plain", Encoding.UTF8);
    }

    [HttpGet("sitemap.xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> Sitemap()
    {
        var entries = await _publicVehicleService.GetSitemapEntriesAsync();
        var lastChange = entries.Max(x => x.LastModified);
        // Teslimatlar sayfası yalnız yayında teslimat varken haritaya girer.
        var deliveriesChange = await _deliveryService.GetLastPublicChangeAsync();

        var urls = StaticPaths
            .Select(path => UrlElement(path, path is "/" or "/ilanlar" ? lastChange : null, null))
            .Concat(deliveriesChange.HasValue ? new[] { UrlElement("/teslimatlar", deliveriesChange, null) } : Array.Empty<XElement>())
            .Concat(entries.Select(x => UrlElement(x.Path, x.LastModified, x.ImageUrl)));

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(SitemapNs + "urlset",
                new XAttribute(XNamespace.Xmlns + "image", ImageNs),
                urls));

        return Content(document.Declaration + Environment.NewLine + document.Root, "application/xml", Encoding.UTF8);
    }

    private XElement UrlElement(string path, DateTime? lastModified, string imageUrl)
    {
        return new XElement(SitemapNs + "url",
            new XElement(SitemapNs + "loc", _urls.Absolute(Request, path)),
            lastModified.HasValue ? new XElement(SitemapNs + "lastmod", lastModified.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)) : null,
            imageUrl != null ? new XElement(ImageNs + "image", new XElement(ImageNs + "loc", _urls.Absolute(Request, imageUrl))) : null);
    }
}
