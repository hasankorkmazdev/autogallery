namespace FatihTasci.Core.Models.Domain.Content;

/// <summary>
/// Ana sayfa girişinin videosu ve afişi (videodan önce görünen ilk kare). Panelde Site İçeriği → Ana Sayfa'dan
/// yüklenir; yüklenmemişse galerinin drone videosu (wwwroot/video) kullanılır. Yalnız okunur: içerik kaydedilirken
/// gönderilse de yok sayılır, video /api/content/hero-video uçlarıyla değişir.
/// </summary>
public class HeroVideoModel
{
    /// <summary>
    /// Masaüstü (genişlik 1024 ve üstü) videosu.
    /// </summary>
    public string VideoHdUrl { get; set; }

    /// <summary>
    /// Telefon ve tablet videosu. Yüklenen videoda sunucu sıkıştırma yapmadığı için masaüstüyle aynı dosyadır.
    /// </summary>
    public string VideoSdUrl { get; set; }

    /// <summary>
    /// Afiş, 1920 px genişlik (WebP).
    /// </summary>
    public string PosterUrl { get; set; }

    /// <summary>
    /// Afiş, 960 px genişlik (WebP).
    /// </summary>
    public string PosterSmallUrl { get; set; }

    /// <summary>
    /// Panelden video yüklendiyse true; false ise varsayılan drone videosudur.
    /// </summary>
    public bool IsCustom { get; set; }
}
