using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain.Public;
using FatihTasci.Core.Providers;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace FatihTasci.Core.Service.Domains;

/// <summary>
/// 1200×630 paylaşım görseli (og:image): fotoğraf, alt kısımda koyu geçiş, başlık, özet ve fiyat; sol üstte logo.
/// Görsel ilk istekte üretilir ve private/share altına içerikten hesaplanan sürümle yazılır; aynı ön ekli eski
/// dosya silinir. Yazı tipleri FatihTasci.Server/Assets/Fonts altındadır (OFL). Çizim SkiaSharp ile yapılır.
/// </summary>
public class ShareImageService : IShareImageService
{
    public const int Width = 1200;
    public const int Height = 630;

    /// <summary>
    /// Görselin düzeni değişince artırılır; tüm paylaşım görselleri yeni sürümle yeniden üretilir.
    /// </summary>
    private const string LayoutVersion = "3";

    private const float Padding = 64f;
    private const string CacheFolder = "share";
    private const int JpegQuality = 86;

    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");
    private static readonly SKColor Background = SKColor.Parse("#0B0B0C");
    private static readonly SKColor TextColor = SKColor.Parse("#F5F3EF");
    private static readonly SKColor MutedColor = SKColor.Parse("#D6D0C6");
    private static readonly SKColor AccentColor = SKColor.Parse("#C8A96A");
    private static readonly SKColor AccentInk = SKColor.Parse("#0B0B0C");
    private static readonly SKSamplingOptions Pixelwise = new(SKFilterMode.Nearest);

    private readonly IMediaStorage _storage;
    private readonly ILogger<ShareImageService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly SKTypeface _display;
    private readonly SKTypeface _body;
    private readonly SKTypeface _bodyBold;

    public ShareImageService(IMediaStorage storage, IWebHostEnvironment env, ILogger<ShareImageService> logger)
    {
        _storage = storage;
        _logger = logger;

        var folder = Path.Combine(env.ContentRootPath, "Assets", "Fonts");
        _display = LoadTypeface(Path.Combine(folder, "CormorantGaramond_600SemiBold.ttf"));
        _body = LoadTypeface(Path.Combine(folder, "Manrope_500Medium.ttf"));
        _bodyBold = LoadTypeface(Path.Combine(folder, "Manrope_700Bold.ttf"));
    }

    public string ComputeVersion(ShareCardModel card)
    {
        var source = string.Join('\u001F', LayoutVersion, card.Title, card.Subtitle, card.PriceText, card.OldPriceText,
            card.Badge, card.BackgroundUrl, card.LogoUrl, card.GalleryName);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)))[..12].ToLowerInvariant();
    }

    public async Task<string> GetOrCreateAsync(ShareCardModel card)
    {
        var fileName = $"{card.CacheName}-{ComputeVersion(card)}.jpg";
        var fullPath = _storage.GetFullPrivatePath($"{CacheFolder}/{fileName}");
        if (File.Exists(fullPath))
            return fullPath;

        await _lock.WaitAsync();
        try
        {
            if (File.Exists(fullPath))
                return fullPath;

            var folder = Path.GetDirectoryName(fullPath);
            Directory.CreateDirectory(folder);

            var tempPath = $"{fullPath}.{Guid.NewGuid():N}.tmp";
            using (var image = Render(card))
            await using (var output = ImageTools.ToJpeg(image, JpegQuality))
            await using (var file = File.Create(tempPath))
            {
                await output.CopyToAsync(file);
            }
            File.Move(tempPath, fullPath, overwrite: true);

            // Aynı kartın eski sürümleri (fiyat, başlık ya da fotoğraf değişmiş)
            foreach (var old in Directory.GetFiles(folder, $"{card.CacheName}-*.jpg"))
            {
                if (!string.Equals(old, fullPath, StringComparison.OrdinalIgnoreCase))
                    TryDelete(old);
            }

            return fullPath;
        }
        finally
        {
            _lock.Release();
        }
    }

    private SKBitmap Render(ShareCardModel card)
    {
        var image = new SKBitmap(new SKImageInfo(Width, Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb()));
        using var canvas = new SKCanvas(image);
        canvas.Clear(Background);

        DrawBackground(canvas, card.BackgroundUrl);

        // Okunabilirlik için üstte hafif, altta güçlü koyu geçiş
        FillGradient(canvas, new SKPoint(0, 0), new SKPoint(0, 170),
            new[] { new SKColor(11, 11, 12, 150), new SKColor(11, 11, 12, 0) }, null);
        FillGradient(canvas, new SKPoint(0, Height * 0.30f), new SKPoint(0, Height),
            new[] { new SKColor(11, 11, 12, 0), new SKColor(11, 11, 12, 185), new SKColor(11, 11, 12, 245) },
            new[] { 0f, 0.5f, 1f });

        DrawBrand(canvas, card);
        DrawBadge(canvas, card.Badge);
        DrawTexts(canvas, card);

        canvas.Flush();
        return image;
    }

    private void DrawBackground(SKCanvas canvas, string backgroundUrl)
    {
        var backgroundPath = ResolveMediaPath(backgroundUrl);
        if (backgroundPath == null)
            return;

        using var photo = ImageTools.DecodeFile(backgroundPath);
        if (photo == null)
        {
            _logger.LogWarning("Paylaşım görselinin fotoğrafı okunamadı: {Path}", backgroundPath);
            return;
        }

        using var cover = ImageTools.Cover(photo, Width, Height);
        canvas.DrawBitmap(cover, 0, 0, Pixelwise, null);
    }

    private static void FillGradient(SKCanvas canvas, SKPoint start, SKPoint end, SKColor[] colors, float[] positions)
    {
        using var shader = SKShader.CreateLinearGradient(start, end, colors, positions, SKShaderTileMode.Clamp);
        using var paint = new SKPaint { Shader = shader };
        canvas.DrawRect(0, 0, Width, Height, paint);
    }

    /// <summary>
    /// Sol üst: logo varsa logo, yoksa galeri adı
    /// </summary>
    private void DrawBrand(SKCanvas canvas, ShareCardModel card)
    {
        var logoPath = ResolveMediaPath(card.LogoUrl);
        if (logoPath != null)
        {
            using var logo = ImageTools.DecodeFile(logoPath);
            if (logo != null)
            {
                using var scaled = ImageTools.Fit(logo, 240, 72);
                canvas.DrawBitmap(scaled, Padding, 52, Pixelwise, null);
                return;
            }
            _logger.LogWarning("Paylaşım görseline logo basılamadı: {Path}", logoPath);
        }

        // Başlık zaten galeri adıysa (site kartı) üstte tekrar yazılmaz.
        if (!string.IsNullOrWhiteSpace(card.GalleryName) && !string.Equals(card.GalleryName, card.Title, StringComparison.Ordinal))
        {
            using var font = CreateFont(_display, 34);
            DrawLines(canvas, new[] { card.GalleryName }, font, Padding, 52, LineHeight(font), TextColor);
        }
    }

    private void DrawBadge(SKCanvas canvas, string badge)
    {
        if (string.IsNullOrWhiteSpace(badge))
            return;

        using var font = CreateFont(_bodyBold, 22);
        var text = badge.ToUpper(Turkish);
        var lineHeight = LineHeight(font);
        var boxWidth = font.MeasureText(text, (SKPaint)null) + 36;
        var boxHeight = lineHeight + 20;
        var boxX = Width - Padding - boxWidth;
        const float boxY = 54f;

        using var fill = new SKPaint { Color = AccentColor, IsAntialias = true };
        canvas.DrawRect(boxX, boxY, boxWidth, boxHeight, fill);
        DrawLines(canvas, new[] { text }, font, boxX + 18, boxY + 10, lineHeight, AccentInk);
    }

    /// <summary>
    /// Alttan yukarı: fiyat, özet satırı, başlık (en çok iki satır), altın çizgi
    /// </summary>
    private void DrawTexts(SKCanvas canvas, ShareCardModel card)
    {
        const float maxWidth = Width - 2 * Padding;
        var y = Height - 58f;

        if (!string.IsNullOrWhiteSpace(card.PriceText))
        {
            using var priceFont = CreateFont(_bodyBold, 46);
            var priceHeight = LineHeight(priceFont);
            var priceWidth = priceFont.MeasureText(card.PriceText, (SKPaint)null);
            y -= priceHeight;
            DrawLines(canvas, new[] { card.PriceText }, priceFont, Padding, y, priceHeight, AccentColor);

            if (!string.IsNullOrWhiteSpace(card.OldPriceText))
            {
                using var oldFont = CreateFont(_body, 28);
                var oldHeight = LineHeight(oldFont);
                var oldWidth = oldFont.MeasureText(card.OldPriceText, (SKPaint)null);
                var oldX = Padding + priceWidth + 24;
                var oldY = y + priceHeight - oldHeight - 4;
                DrawLines(canvas, new[] { card.OldPriceText }, oldFont, oldX, oldY, oldHeight, MutedColor);

                // Üstü çizili: x yüksekliğinin ortasından
                var strikeY = oldY - oldFont.Metrics.Ascent - oldFont.Metrics.XHeight / 2;
                using var strike = new SKPaint { Color = MutedColor, StrokeWidth = 2, IsAntialias = true, Style = SKPaintStyle.Stroke };
                canvas.DrawLine(oldX - 2, strikeY, oldX + oldWidth + 2, strikeY, strike);
            }

            y -= 16;
        }

        if (!string.IsNullOrWhiteSpace(card.Subtitle))
        {
            using var subtitleFont = CreateFont(_body, 26);
            var lineHeight = LineHeight(subtitleFont);
            var lines = FitLines(card.Subtitle, subtitleFont, maxWidth, 1);
            y -= lines.Count * lineHeight;
            DrawLines(canvas, lines, subtitleFont, Padding, y, lineHeight, MutedColor);
            y -= 18;
        }

        if (!string.IsNullOrWhiteSpace(card.Title))
        {
            using var titleFont = CreateFont(_display, string.IsNullOrWhiteSpace(card.PriceText) ? 80 : 62);
            var lineHeight = LineHeight(titleFont, 0.95f);
            var lines = FitLines(card.Title, titleFont, maxWidth, 2);
            y -= lines.Count * lineHeight;
            DrawLines(canvas, lines, titleFont, Padding, y, lineHeight, TextColor);
            y -= 22;
        }

        using var rule = new SKPaint { Color = AccentColor };
        canvas.DrawRect(Padding, y, 56, 2, rule);
    }

    /// <summary>
    /// Satırları üst kenarı top olacak şekilde alt alta yazar.
    /// </summary>
    private static void DrawLines(SKCanvas canvas, IReadOnlyList<string> lines, SKFont font, float x, float top, float lineHeight, SKColor color)
    {
        using var paint = new SKPaint { Color = color, IsAntialias = true };
        var ascent = -font.Metrics.Ascent;
        for (var i = 0; i < lines.Count; i++)
            canvas.DrawText(lines[i], x, top + i * lineHeight + ascent, SKTextAlign.Left, font, paint);
    }

    /// <summary>
    /// Metni kelime kelime maxWidth'e sığan satırlara böler; tek başına sığmayan kelime kendi satırında kalır.
    /// </summary>
    private static List<string> Wrap(string text, SKFont font, float maxWidth)
    {
        var lines = new List<string>();
        var current = string.Empty;
        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = current.Length == 0 ? word : $"{current} {word}";
            if (current.Length > 0 && font.MeasureText(candidate, (SKPaint)null) > maxWidth)
            {
                lines.Add(current);
                current = word;
            }
            else
            {
                current = candidate;
            }
        }

        if (current.Length > 0)
            lines.Add(current);
        return lines;
    }

    /// <summary>
    /// Metin en çok maxLines satıra sığana kadar sondan kelime atar ve "…" ekler.
    /// </summary>
    private static List<string> FitLines(string text, SKFont font, float maxWidth, int maxLines)
    {
        var value = text.Trim();
        var lines = Wrap(value, font, maxWidth);
        if (lines.Count <= maxLines)
            return lines;

        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        while (words.Count > 1)
        {
            words.RemoveAt(words.Count - 1);
            var candidate = string.Join(' ', words).TrimEnd('·', ',', ' ') + "…";
            lines = Wrap(candidate, font, maxWidth);
            if (lines.Count <= maxLines)
                return lines;
        }

        return lines.Take(maxLines).ToList();
    }

    private static float LineHeight(SKFont font, float spacing = 1f)
    {
        var metrics = font.Metrics;
        return (metrics.Descent - metrics.Ascent + metrics.Leading) * spacing;
    }

    private static SKFont CreateFont(SKTypeface typeface, float size)
    {
        return new SKFont(typeface, size) { Edging = SKFontEdging.Antialias, Subpixel = true };
    }

    private static SKTypeface LoadTypeface(string path)
    {
        return SKTypeface.FromFile(path) ?? throw new InvalidOperationException($"Yazı tipi yüklenemedi: {path}");
    }

    /// <summary>
    /// /media/... adresini public depodaki dosya yoluna çevirir. Dosya yoksa null.
    /// </summary>
    private string ResolveMediaPath(string url)
    {
        var prefix = LocalMediaStorage.RequestPath + "/";
        if (string.IsNullOrEmpty(url) || !url.StartsWith(prefix, StringComparison.Ordinal))
            return null;

        var path = _storage.GetFullPublicPath(url[prefix.Length..]);
        return File.Exists(path) ? path : null;
    }

    private void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Eski paylaşım görseli silinemedi: {Path}", path);
        }
    }
}
