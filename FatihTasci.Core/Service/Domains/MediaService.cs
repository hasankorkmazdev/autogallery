using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Common;
using FatihTasci.Core.Models.Domain.Vehicle;
using FatihTasci.Core.Providers;
using FatihTasci.Core.RepositoryBase;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace FatihTasci.Core.Service.Domains;

/// <summary>
/// Araç fotoğrafları, videosu ve ekspertiz raporu. Her fotoğraf için orijinal dosya özel klasörde saklanır,
/// EXIF temizlenmiş 480 / 1280 / 2400 px WebP varyantları /media altında sunulur.
/// Filigran açıksa galeri logosu 1280 ve 2400 varyantlarına basılır; orijinal filigransız kalır.
/// İlk sürümde işleme yükleme isteği içinde yapılır (yük artarsa Hangfire'a taşınır).
/// </summary>
public class MediaService : DomainService, IMediaService
{
    public const long MaxImageBytes = 25 * 1024 * 1024;
    public const long MaxPdfBytes = 20 * 1024 * 1024;

    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    // Küçük varyant liste kartlarında kullanılır, filigran basılmaz.
    private static readonly (string Suffix, int Width, int Quality, bool Watermark)[] Variants =
    {
        ("480", 480, 75, false),
        ("1280", 1280, 80, true),
        ("2400", 2400, 82, true)
    };

    // Filigran: logo fotoğraf genişliğinin %16'sı, kenarlardan %2,5 içeride, sağ altta, yarı saydam.
    private const float WatermarkWidthRatio = 0.16f;
    private const float WatermarkMarginRatio = 0.025f;
    private const float WatermarkOpacity = 0.55f;
    private static readonly int LargestVariantWidth = Variants.Max(x => x.Width);

    // Video afişi: oynatıcı ve galerinin küçük resmi (en uzun kenar; dikey video da sığar).
    private const int VideoPosterSize = 1280;
    private const int VideoPosterSmallSize = 480;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<VehicleEntity> _vehicleRepository;
    private readonly IRepository<VehicleMediaEntity> _mediaRepository;
    private readonly IMediaStorage _storage;
    private readonly ISiteSettingService _settingService;
    private readonly ILogger<MediaService> _logger;

    public MediaService(IUnitOfWork unitOfWork, IMediaStorage storage, ISiteSettingService settingService, ILogger<MediaService> logger)
    {
        _unitOfWork = unitOfWork;
        _vehicleRepository = unitOfWork.GetRepository<VehicleEntity>();
        _mediaRepository = unitOfWork.GetRepository<VehicleMediaEntity>();
        _storage = storage;
        _settingService = settingService;
        _logger = logger;
    }

    public static VehicleMediaModel ToModel(VehicleMediaEntity x, IMediaStorage storage) => new()
    {
        Id = x.Id,
        SmallUrl = storage.ToUrl(x.SmallPath),
        MediumUrl = storage.ToUrl(x.MediumPath),
        LargeUrl = storage.ToUrl(x.LargePath),
        Width = x.Width,
        Height = x.Height,
        SortOrder = x.SortOrder,
        IsCover = x.IsCover
    };

    public async Task<ServiceResultModel<List<VehicleMediaModel>>> UploadVehicleImagesAsync(Guid vehicleId, IFormFileCollection files)
    {
        if (!await _vehicleRepository.ExistsAsync(x => x.Id == vehicleId))
            return Result<List<VehicleMediaModel>>(null, "Araç bulunamadı.", 404);

        if (files == null || files.Count == 0)
            return Result<List<VehicleMediaModel>>(null, "Yüklenecek fotoğraf seçin.", 400);

        // Araç başına fotoğraf sayısı sınırsızdır; yalnız dosya ve istek boyutu sınırlıdır.
        var existingCount = await _mediaRepository.CountAsync(x => x.VehicleId == vehicleId);

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
                return Result<List<VehicleMediaModel>>(null, $"{file.FileName}: yalnızca JPG, PNG ve WebP yüklenebilir.", 400);
            if (file.Length == 0 || file.Length > MaxImageBytes)
                return Result<List<VehicleMediaModel>>(null, $"{file.FileName}: dosya boyutu en fazla 25 MB olabilir.", 400);
        }

        var nextOrder = existingCount == 0
            ? 0
            : await _mediaRepository.GetQuery().Where(x => x.VehicleId == vehicleId).MaxAsync(x => x.SortOrder) + 1;
        var hasCover = await _mediaRepository.ExistsAsync(x => x.VehicleId == vehicleId && x.IsCover);
        var writtenPublic = new List<string>();
        var writtenPrivate = new List<string>();
        using var watermark = await LoadWatermarkAsync();

        try
        {
            foreach (var file in files)
            {
                var media = await ProcessImageAsync(vehicleId, file, watermark, writtenPublic, writtenPrivate);
                if (media == null)
                {
                    Cleanup(writtenPublic, writtenPrivate);
                    return Result<List<VehicleMediaModel>>(null, $"{file.FileName}: dosya okunamadı, geçerli bir fotoğraf değil.", 400);
                }

                media.SortOrder = nextOrder++;
                media.IsCover = !hasCover;
                hasCover = true;
                _mediaRepository.Insert(media);
            }

            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            Cleanup(writtenPublic, writtenPrivate);
            throw;
        }

        return Result(await LoadMediaAsync(vehicleId), $"{files.Count} fotoğraf yüklendi.");
    }

    public async Task<ServiceResultModel<List<VehicleMediaModel>>> ReorderAsync(Guid vehicleId, VehicleMediaOrderRequestModel request)
    {
        var media = await _mediaRepository.GetQuery(disableTracking: false).Where(x => x.VehicleId == vehicleId).ToListAsync();
        if (media.Count != request.MediaIds.Count || media.Any(x => !request.MediaIds.Contains(x.Id)))
            return Result<List<VehicleMediaModel>>(null, "Fotoğraf listesi güncel değil, sayfayı yenileyin.", 400);

        foreach (var item in media)
            item.SortOrder = request.MediaIds.IndexOf(item.Id);

        await _unitOfWork.SaveChangesAsync();
        return Result(await LoadMediaAsync(vehicleId), "Sıralama kaydedildi.");
    }

    public async Task<ServiceResultModel<List<VehicleMediaModel>>> SetCoverAsync(Guid vehicleId, Guid mediaId)
    {
        var media = await _mediaRepository.GetQuery(disableTracking: false).Where(x => x.VehicleId == vehicleId).ToListAsync();
        if (media.All(x => x.Id != mediaId))
            return Result<List<VehicleMediaModel>>(null, "Fotoğraf bulunamadı.", 404);

        foreach (var item in media)
            item.IsCover = item.Id == mediaId;

        await _unitOfWork.SaveChangesAsync();
        return Result(await LoadMediaAsync(vehicleId), "Kapak fotoğrafı seçildi.");
    }

    public async Task<ServiceResultModel<List<VehicleMediaModel>>> DeleteAsync(Guid vehicleId, Guid mediaId)
    {
        var media = await _mediaRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == mediaId && x.VehicleId == vehicleId, disableTracking: false);
        if (media == null)
            return Result<List<VehicleMediaModel>>(null, "Fotoğraf bulunamadı.", 404);

        _mediaRepository.Delete(media);

        // Kapak silindiyse sıradaki ilk fotoğraf kapak olur.
        if (media.IsCover)
        {
            var next = await _mediaRepository.GetQuery(disableTracking: false)
                .Where(x => x.VehicleId == vehicleId && x.Id != mediaId)
                .OrderBy(x => x.SortOrder)
                .FirstOrDefaultAsync();
            if (next != null)
                next.IsCover = true;
        }

        await _unitOfWork.SaveChangesAsync();
        Cleanup(new List<string> { media.SmallPath, media.MediumPath, media.LargePath }, new List<string> { media.OriginalPath });

        return Result(await LoadMediaAsync(vehicleId), "Fotoğraf silindi.");
    }

    public async Task<ServiceResultModel<string>> UploadExpertiseReportAsync(Guid vehicleId, IFormFile file)
    {
        var vehicle = await _vehicleRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == vehicleId, disableTracking: false);
        if (vehicle == null)
            return Result<string>(null, "Araç bulunamadı.", 404);

        if (file == null || file.Length == 0)
            return Result<string>(null, "Yüklenecek PDF dosyasını seçin.", 400);
        if (file.Length > MaxPdfBytes)
            return Result<string>(null, "Ekspertiz raporu en fazla 20 MB olabilir.", 400);
        if (!await IsPdfAsync(file))
            return Result<string>(null, "Ekspertiz raporu PDF olmalıdır.", 400);

        var oldPath = vehicle.ExpertiseReportPath;
        var path = $"vehicles/{vehicleId:N}/ekspertiz-{Guid.NewGuid():N}.pdf";
        await using (var stream = file.OpenReadStream())
            await _storage.SavePublicAsync(path, stream);

        vehicle.ExpertiseReportPath = path;
        await _unitOfWork.SaveChangesAsync();
        _storage.DeletePublic(oldPath);

        return Result(_storage.ToUrl(path), "Ekspertiz raporu yüklendi.");
    }

    public async Task<ServiceResultModel> DeleteExpertiseReportAsync(Guid vehicleId)
    {
        var vehicle = await _vehicleRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == vehicleId, disableTracking: false);
        if (vehicle == null)
            return Result("Araç bulunamadı.", 404);

        var oldPath = vehicle.ExpertiseReportPath;
        vehicle.ExpertiseReportPath = null;
        await _unitOfWork.SaveChangesAsync();
        _storage.DeletePublic(oldPath);

        return Result(message: "Ekspertiz raporu silindi.");
    }

    public static VideoFileModel ToVideoModel(VehicleEntity x, IMediaStorage storage)
    {
        if (string.IsNullOrEmpty(x.VideoPath))
            return null;

        return new VideoFileModel
        {
            Url = storage.ToUrl(x.VideoPath),
            PosterUrl = storage.ToUrl(x.VideoPosterPath),
            PosterSmallUrl = storage.ToUrl(x.VideoPosterSmallPath),
            Width = x.VideoWidth ?? 0,
            Height = x.VideoHeight ?? 0
        };
    }

    /// <summary>
    /// Video olduğu gibi saklanır (sunucuda sıkıştırma ve filigran yok). Afiş panelde tarayıcının videodan aldığı ilk
    /// karedir; 1280 ve 480 px WebP'ye çevrilir. Yeni video eskisinin yerine geçer, eski dosyalar silinir.
    /// </summary>
    public async Task<ServiceResultModel<VideoFileModel>> UploadVehicleVideoAsync(Guid vehicleId, IFormFile video, IFormFile poster)
    {
        var vehicle = await _vehicleRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == vehicleId, disableTracking: false);
        if (vehicle == null)
            return Result<VideoFileModel>(null, "Araç bulunamadı.", 404);

        var error = await VideoUpload.ValidateAsync(video, poster);
        if (error != null)
            return Result<VideoFileModel>(null, error, 400);

        using var posterImage = VideoUpload.DecodePoster(poster, VideoPosterSize, out var width, out var height);
        if (posterImage == null)
        {
            _logger.LogWarning("Araç videosunun afişi okunamadı: {VehicleId}", vehicleId);
            return Result<VideoFileModel>(null, VideoUpload.PosterUnreadableMessage, 400);
        }

        var oldPaths = VideoPaths(vehicle);
        var basePath = $"vehicles/{vehicleId:N}/video-{Guid.NewGuid():N}";
        var written = new List<string>();
        try
        {
            var posterPath = $"{basePath}-poster-{VideoPosterSize}.webp";
            await VideoUpload.SavePosterAsync(_storage, posterImage, posterPath, VideoPosterSize);
            written.Add(posterPath);

            var posterSmallPath = $"{basePath}-poster-{VideoPosterSmallSize}.webp";
            await VideoUpload.SavePosterAsync(_storage, posterImage, posterSmallPath, VideoPosterSmallSize, 75);
            written.Add(posterSmallPath);

            var videoPath = $"{basePath}.mp4";
            await using (var stream = video.OpenReadStream())
                await _storage.SavePublicAsync(videoPath, stream);
            written.Add(videoPath);

            vehicle.VideoPath = videoPath;
            vehicle.VideoPosterPath = posterPath;
            vehicle.VideoPosterSmallPath = posterSmallPath;
            vehicle.VideoWidth = width;
            vehicle.VideoHeight = height;
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            Cleanup(written, Array.Empty<string>());
            throw;
        }

        Cleanup(oldPaths, Array.Empty<string>());
        return Result(ToVideoModel(vehicle, _storage), "Video yüklendi.");
    }

    public async Task<ServiceResultModel> DeleteVehicleVideoAsync(Guid vehicleId)
    {
        var vehicle = await _vehicleRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == vehicleId, disableTracking: false);
        if (vehicle == null)
            return Result("Araç bulunamadı.", 404);

        var oldPaths = VideoPaths(vehicle);
        vehicle.VideoPath = null;
        vehicle.VideoPosterPath = null;
        vehicle.VideoPosterSmallPath = null;
        vehicle.VideoWidth = null;
        vehicle.VideoHeight = null;
        await _unitOfWork.SaveChangesAsync();
        Cleanup(oldPaths, Array.Empty<string>());

        return Result(message: "Video silindi.");
    }

    private static List<string> VideoPaths(VehicleEntity vehicle)
    {
        return new List<string> { vehicle.VideoPath, vehicle.VideoPosterPath, vehicle.VideoPosterSmallPath };
    }

    public async Task RegenerateAllVariantsAsync(Action<int> onStart, Action<bool> onItem, CancellationToken cancellationToken)
    {
        var mediaIds = await _mediaRepository.GetQuery()
            .OrderBy(x => x.VehicleId)
            .ThenBy(x => x.SortOrder)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        onStart(mediaIds.Count);

        using var watermark = await LoadWatermarkAsync();
        foreach (var mediaId in mediaIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            onItem(await RegenerateVariantsAsync(mediaId, watermark));
        }
    }

    /// <summary>
    /// Bir fotoğrafın varyantlarını orijinalden yeni adlarla üretir, kaydı günceller ve eski dosyaları siler.
    /// Medya uzun süre önbelleğe alındığı için aynı adın üzerine yazılmaz.
    /// </summary>
    private async Task<bool> RegenerateVariantsAsync(Guid mediaId, SKBitmap watermark)
    {
        var media = await _mediaRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == mediaId, disableTracking: false);
        if (media == null)
            return true; // İş sürerken silinmiş

        var originalPath = _storage.GetFullPrivatePath(media.OriginalPath);
        if (!File.Exists(originalPath))
        {
            _logger.LogWarning("Orijinal fotoğraf bulunamadı: {Path}", media.OriginalPath);
            return false;
        }

        var written = new List<string>();
        try
        {
            using var image = ImageTools.DecodeFile(originalPath, LargestVariantWidth);
            if (image == null)
            {
                _logger.LogWarning("Orijinal fotoğraf okunamadı: {Path}", media.OriginalPath);
                return false;
            }

            var (small, medium, large) = await WriteVariantsAsync(_storage, image, media.VehicleId, media.Id, watermark, written);
            var oldPaths = new List<string> { media.SmallPath, media.MediumPath, media.LargePath };
            media.SmallPath = small;
            media.MediumPath = medium;
            media.LargePath = large;
            await _unitOfWork.SaveChangesAsync();

            Cleanup(oldPaths, Array.Empty<string>());
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fotoğraf varyantları yeniden üretilemedi: {MediaId}", mediaId);
            Cleanup(written, Array.Empty<string>());
            return false;
        }
    }

    /// <summary>
    /// Filigran açık ve logo yüklüyse logoyu döner, aksi halde null. Çağıran dispose eder.
    /// </summary>
    private async Task<SKBitmap> LoadWatermarkAsync()
    {
        var settings = await _settingService.GetAsync();
        if (!settings.WatermarkEnabled || string.IsNullOrEmpty(settings.LogoPath))
            return null;

        var fullPath = _storage.GetFullPublicPath(settings.LogoPath);
        if (!File.Exists(fullPath))
            return null;

        var logo = ImageTools.DecodeFile(fullPath);
        if (logo == null)
            _logger.LogWarning("Filigran logosu okunamadı: {Path}", settings.LogoPath);
        return logo;
    }

    /// <summary>
    /// Fotoğrafı okur; orijinali özel klasöre, 480 / 1280 / 2400 px WebP varyantlarını /media altına yazar (watermark
    /// null değilse 1280 ve 2400'e basılır). Panel yüklemesi ve geliştirme seed'i (DbSeeder) aynı yolu kullanır.
    /// Okunamayan dosyada null döner. Yazılan dosyalar listelere eklenir (kaydetme başarısız olursa çağıran siler);
    /// sıra ve kapak çağırana kalır.
    /// </summary>
    public static async Task<VehicleMediaEntity> WriteImageAsync(IMediaStorage storage, Guid vehicleId, Stream source, string extension,
        SKBitmap watermark, List<string> writtenPublic, List<string> writtenPrivate)
    {
        var mediaId = Guid.NewGuid();

        await using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer);

        // En büyük varyantın altına inmeden çözülür; yön uygulanır, yeniden kodlanan varyantlarda metadata kalmaz.
        buffer.Position = 0;
        using var image = ImageTools.Decode(buffer, LargestVariantWidth, 0, out var originalWidth, out var originalHeight);
        if (image == null)
            return null;

        var originalPath = $"originals/vehicles/{vehicleId:N}/{mediaId:N}{extension.ToLowerInvariant()}";
        buffer.Position = 0;
        await storage.SavePrivateAsync(originalPath, buffer);
        writtenPrivate.Add(originalPath);

        var entity = new VehicleMediaEntity
        {
            Id = mediaId,
            VehicleId = vehicleId,
            OriginalPath = originalPath,
            Width = originalWidth,
            Height = originalHeight
        };

        (entity.SmallPath, entity.MediumPath, entity.LargePath) =
            await WriteVariantsAsync(storage, image, vehicleId, mediaId, watermark, writtenPublic);

        return entity;
    }

    private static async Task<(string Small, string Medium, string Large)> WriteVariantsAsync(
        IMediaStorage storage, SKBitmap image, Guid vehicleId, Guid mediaId, SKBitmap watermark, List<string> writtenPublic)
    {
        var revision = Guid.NewGuid().ToString("N")[..8];
        string small = null, medium = null, large = null;

        foreach (var (suffix, width, quality, applyWatermark) in Variants)
        {
            var path = $"vehicles/{vehicleId:N}/{mediaId:N}-{revision}-{suffix}.webp";
            using var variant = ImageTools.ScaleToWidth(image, width);
            if (applyWatermark && watermark != null)
                ImageTools.DrawWatermark(variant, watermark, WatermarkWidthRatio, WatermarkMarginRatio, WatermarkOpacity);

            await using var output = ImageTools.ToWebp(variant, quality);
            await storage.SavePublicAsync(path, output);
            writtenPublic.Add(path);

            switch (suffix)
            {
                case "480": small = path; break;
                case "1280": medium = path; break;
                default: large = path; break;
            }
        }

        return (small, medium, large);
    }

    private async Task<VehicleMediaEntity> ProcessImageAsync(Guid vehicleId, IFormFile file, SKBitmap watermark, List<string> writtenPublic, List<string> writtenPrivate)
    {
        await using var stream = file.OpenReadStream();
        var media = await WriteImageAsync(_storage, vehicleId, stream, Path.GetExtension(file.FileName), watermark, writtenPublic, writtenPrivate);
        if (media == null)
            _logger.LogWarning("Fotoğraf okunamadı: {FileName}", file.FileName);
        return media;
    }

    private async Task<List<VehicleMediaModel>> LoadMediaAsync(Guid vehicleId)
    {
        var media = await _mediaRepository.GetQuery()
            .Where(x => x.VehicleId == vehicleId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();
        return media.Select(x => ToModel(x, _storage)).ToList();
    }

    private void Cleanup(IEnumerable<string> publicPaths, IEnumerable<string> privatePaths)
    {
        foreach (var path in publicPaths)
            TryDelete(() => _storage.DeletePublic(path), path);
        foreach (var path in privatePaths)
            TryDelete(() => _storage.DeletePrivate(path), path);
    }

    private void TryDelete(Action delete, string path)
    {
        try
        {
            delete();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Dosya silinemedi: {Path}", path);
        }
    }

    private static async Task<bool> IsPdfAsync(IFormFile file)
    {
        var header = new byte[5];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(header);
        return read == 5 && header[0] == '%' && header[1] == 'P' && header[2] == 'D' && header[3] == 'F' && header[4] == '-';
    }
}
