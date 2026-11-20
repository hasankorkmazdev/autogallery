using FatihTasci.Core.Providers;
using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace FatihTasci.Core.Helpers;

/// <summary>
/// Panelden yüklenen videolar (ana sayfa girişi, araç, teslimat). Sunucuda video işleme yoktur: MP4 olduğu gibi
/// saklanır, panel web'e uygun H.264 MP4 ister. Afiş panelde tarayıcının videodan aldığı ilk karedir (tarayıcının
/// açamadığı video yüklenmez); sunucu onu WebP'ye çevirir. /media bir yıl önbelleğe alındığı için her yüklemede yeni ad verilir.
/// </summary>
public static class VideoUpload
{
    public const long MaxVideoBytes = 100L * 1024 * 1024;
    public const long MaxPosterBytes = 10L * 1024 * 1024;

    /// <summary>
    /// Video + afiş isteğinin boyut sınırı (multipart başlıkları için 1 MB pay).
    /// </summary>
    public const long MaxRequestBytes = MaxVideoBytes + MaxPosterBytes + 1024 * 1024;

    public const string PosterUnreadableMessage = "Videonun ilk karesi okunamadı. Sayfayı yenileyip tekrar deneyin.";

    /// <summary>
    /// Video ve afişi denetler; sorun varsa kullanıcıya gösterilecek metni, yoksa null döner.
    /// </summary>
    public static async Task<string> ValidateAsync(IFormFile video, IFormFile poster)
    {
        if (video == null || video.Length == 0)
            return "Yüklenecek videoyu seçin.";
        if (video.Length > MaxVideoBytes)
            return $"Video en fazla {MaxVideoBytes / 1024 / 1024} MB olabilir.";
        if (!string.Equals(Path.GetExtension(video.FileName), ".mp4", StringComparison.OrdinalIgnoreCase) || !await IsMp4Async(video))
            return "Video MP4 (H.264) olmalıdır.";
        if (poster == null || poster.Length == 0 || poster.Length > MaxPosterBytes)
            return "Videonun ilk karesi alınamadı. Sayfayı yenileyip tekrar deneyin.";
        return null;
    }

    /// <summary>
    /// Afişi çözer (en çok fitSize × fitSize kutusuna küçültülerek); width / height afişin boyutudur, oranı videoyla
    /// aynıdır. Okunamazsa null döner. Dönen bitmap'i çağıran dispose eder.
    /// </summary>
    public static SKBitmap DecodePoster(IFormFile poster, int fitSize, out int width, out int height)
    {
        using var input = poster.OpenReadStream();
        return ImageTools.Decode(input, fitSize, fitSize, out width, out height);
    }

    /// <summary>
    /// Afişi en çok size × size kutusuna sığdırıp WebP olarak yazar.
    /// </summary>
    public static async Task SavePosterAsync(IMediaStorage storage, SKBitmap poster, string path, int size, int quality = 80)
    {
        using var variant = ImageTools.Fit(poster, size, size);
        await using var output = ImageTools.ToWebp(variant, quality);
        await storage.SavePublicAsync(path, output);
    }

    /// <summary>
    /// MP4 (ISO BMFF) dosyası 4. bayttan itibaren "ftyp" kutusuyla başlar.
    /// </summary>
    public static async Task<bool> IsMp4Async(IFormFile file)
    {
        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);
        return read == header.Length && header[4] == 'f' && header[5] == 't' && header[6] == 'y' && header[7] == 'p';
    }
}
