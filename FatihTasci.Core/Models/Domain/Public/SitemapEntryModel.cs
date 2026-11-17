namespace FatihTasci.Core.Models.Domain.Public;

/// <summary>
/// sitemap.xml satırı. Yol site köküne göredir (/arac/...); tam adresi sunucu ekler.
/// </summary>
public class SitemapEntryModel
{
    public string Path { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Sayfanın ana fotoğrafı (/media/...), görsel sitemap'i için. Yoksa null.
    /// </summary>
    public string ImageUrl { get; set; }
}
