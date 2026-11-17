using SkiaSharp;

namespace FatihTasci.Core.Helpers;

/// <summary>
/// Fotoğraf işleme (SkiaSharp, MIT): çözme, yüksek kaliteli küçültme, WebP / JPEG / PNG kodlama ve filigran.
/// Çözülen fotoğrafa EXIF yönü uygulanır ve renkler sRGB'ye çevrilir; yeniden kodlanan dosyaya EXIF / XMP / IPTC
/// (konum dahil) yazılmaz. Dönen her SKBitmap'i çağıran dispose eder.
/// </summary>
public static class ImageTools
{
    /// <summary>
    /// Çözülen fotoğrafın en çok piksel sayısı (~256 MB bellek). Daha büyüğü (ör. dev PNG) okunamaz sayılır;
    /// JPEG ve WebP gerektiğinden büyükse zaten küçültülerek çözülür.
    /// </summary>
    public const long MaxDecodedPixels = 64_000_000;

    private static readonly SKSamplingOptions HalfStep = new(SKFilterMode.Linear, SKMipmapMode.None);
    private static readonly SKSamplingOptions FinalStep = new(SKCubicResampler.Mitchell);

    /// <summary>
    /// Fotoğrafı çözer; dosya okunabilen bir görsel değilse ya da çok büyükse null döner.
    /// fitWidth / fitHeight: çağıran görseli en çok bu kutuya küçültecekse verilir (0 = o yönde sınır yok); JPEG ve
    /// WebP bu kutunun altına inmeden küçültülerek çözülür. originalWidth / originalHeight: yönü uygulanmış dosya boyutu.
    /// </summary>
    public static SKBitmap Decode(Stream stream, int fitWidth, int fitHeight, out int originalWidth, out int originalHeight)
    {
        originalWidth = 0;
        originalHeight = 0;

        using var data = SKData.Create(stream);
        if (data == null)
            return null;
        using var codec = SKCodec.Create(data);
        if (codec == null || codec.Info.Width <= 0 || codec.Info.Height <= 0)
            return null;

        var origin = codec.EncodedOrigin;
        var swap = SwapsAxes(origin);
        originalWidth = swap ? codec.Info.Height : codec.Info.Width;
        originalHeight = swap ? codec.Info.Width : codec.Info.Height;

        var full = new SKSizeI(codec.Info.Width, codec.Info.Height);
        var scale = FitScale(originalWidth, originalHeight, fitWidth, fitHeight);
        var size = scale < 1f ? codec.GetScaledDimensions(scale) : full;

        var bitmap = DecodeAt(codec, size);
        if (bitmap == null && size != full)
            bitmap = DecodeAt(codec, full); // Bu biçim küçültülerek çözülemiyor

        return bitmap == null ? null : Orient(bitmap, origin);
    }

    public static SKBitmap Decode(Stream stream, int fitWidth = 0, int fitHeight = 0)
    {
        return Decode(stream, fitWidth, fitHeight, out _, out _);
    }

    public static SKBitmap DecodeFile(string path, int fitWidth = 0, int fitHeight = 0)
    {
        using var stream = File.OpenRead(path);
        return Decode(stream, fitWidth, fitHeight);
    }

    /// <summary>
    /// Genişliği en çok maxWidth olacak şekilde orantılı küçültür (büyütmez). Her zaman yeni bitmap döner.
    /// </summary>
    public static SKBitmap ScaleToWidth(SKBitmap source, int maxWidth)
    {
        return Fit(source, maxWidth, 0);
    }

    /// <summary>
    /// maxWidth × maxHeight kutusuna sığacak şekilde orantılı küçültür (büyütmez; 0 = o yönde sınır yok).
    /// Her zaman yeni bitmap döner.
    /// </summary>
    public static SKBitmap Fit(SKBitmap source, int maxWidth, int maxHeight)
    {
        var scale = FitScale(source.Width, source.Height, maxWidth, maxHeight);
        if (scale >= 1f)
            return source.Copy();

        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));
        return Resize(source, width, height);
    }

    /// <summary>
    /// width × height alanını tamamen dolduracak şekilde ölçekler ve ortadan kırpar.
    /// </summary>
    public static SKBitmap Cover(SKBitmap source, int width, int height)
    {
        var scale = Math.Max((float)width / source.Width, (float)height / source.Height);
        var scaledWidth = Math.Max(width, (int)Math.Ceiling(source.Width * scale));
        var scaledHeight = Math.Max(height, (int)Math.Ceiling(source.Height * scale));

        using var scaled = scaledWidth == source.Width && scaledHeight == source.Height
            ? source.Copy()
            : Resize(source, scaledWidth, scaledHeight);

        var result = new SKBitmap(new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb()));
        using var canvas = new SKCanvas(result);
        canvas.Clear(SKColors.Transparent);
        canvas.DrawBitmap(scaled, -(scaledWidth - width) / 2f, -(scaledHeight - height) / 2f, new SKSamplingOptions(SKFilterMode.Nearest), null);
        return result;
    }

    /// <summary>
    /// Logoyu hedefin sağ altına yarı saydam basar: genişliğin widthRatio'su kadar, kenarlardan marginRatio içeride.
    /// Hedef çok küçükse basılmaz.
    /// </summary>
    public static void DrawWatermark(SKBitmap target, SKBitmap logo, float widthRatio, float marginRatio, float opacity)
    {
        var maxWidth = Math.Max(48, (int)(target.Width * widthRatio));
        var margin = (int)(target.Width * marginRatio);

        using var scaled = Fit(logo, maxWidth, maxWidth);
        if (scaled.Width + margin * 2 > target.Width || scaled.Height + margin * 2 > target.Height)
            return;

        using var canvas = new SKCanvas(target);
        using var paint = new SKPaint { Color = SKColors.White.WithAlpha((byte)Math.Round(opacity * 255)) };
        canvas.DrawBitmap(scaled, target.Width - scaled.Width - margin, target.Height - scaled.Height - margin,
            new SKSamplingOptions(SKFilterMode.Nearest), paint);
    }

    public static MemoryStream ToWebp(SKBitmap bitmap, int quality)
    {
        using var pixmap = bitmap.PeekPixels();
        using var data = pixmap.Encode(new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, quality));
        return ToStream(data, "WebP");
    }

    public static MemoryStream ToJpeg(SKBitmap bitmap, int quality)
    {
        using var pixmap = bitmap.PeekPixels();
        using var data = pixmap.Encode(new SKJpegEncoderOptions(quality, SKJpegEncoderDownsample.Downsample420, SKJpegEncoderAlphaOption.Ignore));
        return ToStream(data, "JPEG");
    }

    public static MemoryStream ToPng(SKBitmap bitmap)
    {
        using var pixmap = bitmap.PeekPixels();
        using var data = pixmap.Encode(SKPngEncoderOptions.Default);
        return ToStream(data, "PNG");
    }

    private static MemoryStream ToStream(SKData data, string format)
    {
        if (data == null)
            throw new InvalidOperationException($"Görsel {format} olarak kodlanamadı.");
        return new MemoryStream(data.ToArray(), writable: false);
    }

    /// <summary>
    /// Kaliteli küçültme: 2 kattan fazla küçültmede önce yarıya indirerek (2×2 ortalama) ilerler, son adım Mitchell.
    /// Doğrudan tek adımda küçültmek ince desenlerde (ızgara, jant) titreşim / tırtık bırakır.
    /// </summary>
    private static SKBitmap Resize(SKBitmap source, int width, int height)
    {
        var current = source;
        try
        {
            while (current.Width / 2 >= width && current.Height / 2 >= height)
            {
                var half = current.Resize(current.Info.WithSize(current.Width / 2, current.Height / 2), HalfStep)
                    ?? throw new InvalidOperationException("Görsel küçültülemedi.");
                if (!ReferenceEquals(current, source))
                    current.Dispose();
                current = half;
            }

            return current.Resize(current.Info.WithSize(width, height), FinalStep)
                ?? throw new InvalidOperationException("Görsel küçültülemedi.");
        }
        finally
        {
            if (!ReferenceEquals(current, source))
                current.Dispose();
        }
    }

    private static SKBitmap DecodeAt(SKCodec codec, SKSizeI size)
    {
        if ((long)size.Width * size.Height > MaxDecodedPixels)
            return null;

        var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
        var bitmap = new SKBitmap(info);
        if (bitmap.GetPixels() == IntPtr.Zero || codec.GetPixels(info, bitmap.GetPixels()) != SKCodecResult.Success)
        {
            bitmap.Dispose();
            return null;
        }

        return bitmap;
    }

    /// <summary>
    /// Kutuya sığdırmak için gereken ölçek (1 = küçültme yok). Sıfır verilen yön sınırlanmaz.
    /// </summary>
    private static float FitScale(int width, int height, int maxWidth, int maxHeight)
    {
        var scale = 1f;
        if (maxWidth > 0)
            scale = Math.Min(scale, (float)maxWidth / width);
        if (maxHeight > 0)
            scale = Math.Min(scale, (float)maxHeight / height);
        return scale;
    }

    private static bool SwapsAxes(SKEncodedOrigin origin)
    {
        return origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
    }

    /// <summary>
    /// EXIF yönünü uygular (telefon fotoğrafları çoğunlukla yan kaydedilir). Yön düzse aynı bitmap döner,
    /// değilse yenisi döner ve kaynak dispose edilir.
    /// </summary>
    private static SKBitmap Orient(SKBitmap source, SKEncodedOrigin origin)
    {
        if (origin == SKEncodedOrigin.TopLeft)
            return source;

        var w = source.Width;
        var h = source.Height;
        var swap = SwapsAxes(origin);
        // Kaynak (x, y) → hedef: x' = ScaleX·x + SkewX·y + TransX, y' = SkewY·x + ScaleY·y + TransY
        var matrix = origin switch
        {
            SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, w, 0, 1, 0, 0, 0, 1),    // yatay ayna
            SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, w, 0, -1, h, 0, 0, 1), // 180°
            SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, h, 0, 0, 1),   // dikey ayna
            SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),       // köşegene göre ayna
            SKEncodedOrigin.RightTop => new SKMatrix(0, -1, h, 1, 0, 0, 0, 0, 1),     // saat yönünde 90°
            SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, h, -1, 0, w, 0, 0, 1), // ters köşegene göre ayna
            SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, w, 0, 0, 1),   // saatin tersine 90°
            _ => SKMatrix.Identity
        };

        var info = source.Info.WithSize(swap ? h : w, swap ? w : h);
        var result = new SKBitmap(info);
        using (var canvas = new SKCanvas(result))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.SetMatrix(in matrix);
            canvas.DrawBitmap(source, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest), null);
        }

        source.Dispose();
        return result;
    }
}
