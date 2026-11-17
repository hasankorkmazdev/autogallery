namespace FatihTasci.Core.Models.Domain.Public;

/// <summary>
/// 1200×630 paylaşım görselinin (og:image) içeriği. Metinler ekranda görüneceği gibi hazırlanır;
/// aynı içerik aynı dosyayı verir (sürüm içerikten hesaplanır).
/// </summary>
public class ShareCardModel
{
    /// <summary>
    /// Önbellek dosyasının ön eki (vehicle-100012, site). Aynı ön ekli eski dosyalar yenisi yazılınca silinir.
    /// </summary>
    public string CacheName { get; set; }

    public string Title { get; set; }

    /// <summary>
    /// Başlığın altındaki özet satırı (yıl · km · yakıt · vites)
    /// </summary>
    public string Subtitle { get; set; }

    public string PriceText { get; set; }

    /// <summary>
    /// Kampanya varsa eski fiyat (üstü çizili gösterilir)
    /// </summary>
    public string OldPriceText { get; set; }

    /// <summary>
    /// Sağ üstteki rozet (Rezerve, Satıldı, Kampanya)
    /// </summary>
    public string Badge { get; set; }

    /// <summary>
    /// Arka plan fotoğrafı, /media/... adresi. Boşsa düz koyu zemin.
    /// </summary>
    public string BackgroundUrl { get; set; }

    /// <summary>
    /// Firma logosu, /media/... adresi. Boşsa sol üstte galeri adı yazılır.
    /// </summary>
    public string LogoUrl { get; set; }

    public string GalleryName { get; set; }
}
