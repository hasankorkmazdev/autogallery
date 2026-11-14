using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Vehicle;
using FatihTasci.Core.Providers;
using FatihTasci.Core.RepositoryBase;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace FatihTasci.Core.Service.Domains;

/// <summary>
/// Araç fotoğrafları ve ekspertiz raporu. Her fotoğraf için orijinal dosya özel klasörde saklanır,
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
    private async Task<bool> RegenerateVariantsAsync(Guid mediaId, Image watermark)
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
            using var image = await Image.LoadAsync(originalPath);
            PrepareImage(image);

            var (small, medium, large) = await WriteVariantsAsync(image, media.VehicleId, media.Id, watermark, written);
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
    private async Task<Image> LoadWatermarkAsync()
    {
        var settings = await _settingService.GetAsync();
        if (!settings.WatermarkEnabled || string.IsNullOrEmpty(settings.LogoPath))
            return null;

        var fullPath = _storage.GetFullPublicPath(settings.LogoPath);
        if (!File.Exists(fullPath))
            return null;

        try
        {
            return await Image.LoadAsync(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Filigran logosu okunamadı: {Path}", settings.LogoPath);
            return null;
        }
    }

    private static void PrepareImage(Image image)
    {
        // Telefon fotoğraflarındaki yönü uygula, ardından konum dahil tüm metadata'yı temizle.
        image.Mutate(x => x.AutoOrient());
        image.Metadata.ExifProfile = null;
        image.Metadata.XmpProfile = null;
        image.Metadata.IptcProfile = null;
    }

    private async Task<(string Small, string Medium, string Large)> WriteVariantsAsync(
        Image image, Guid vehicleId, Guid mediaId, Image watermark, List<string> writtenPublic)
    {
        var revision = Guid.NewGuid().ToString("N")[..8];
        string small = null, medium = null, large = null;

        foreach (var (suffix, width, quality, applyWatermark) in Variants)
        {
            var path = $"vehicles/{vehicleId:N}/{mediaId:N}-{revision}-{suffix}.webp";
            using var variant = image.Width > width
                ? image.Clone(x => x.Resize(new ResizeOptions { Size = new Size(width, 0), Mode = ResizeMode.Max }))
                : image.Clone(_ => { });

            if (applyWatermark && watermark != null)
                ApplyWatermark(variant, watermark);

            await using var output = new MemoryStream();
            await variant.SaveAsWebpAsync(output, new WebpEncoder { Quality = quality });
            output.Position = 0;
            await _storage.SavePublicAsync(path, output);
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

    private static void ApplyWatermark(Image target, Image logo)
    {
        var maxWidth = Math.Max(48, (int)(target.Width * WatermarkWidthRatio));
        var margin = (int)(target.Width * WatermarkMarginRatio);

        using var scaled = logo.Clone(x => x.Resize(new ResizeOptions { Size = new Size(maxWidth, maxWidth), Mode = ResizeMode.Max }));
        if (scaled.Width + margin * 2 > target.Width || scaled.Height + margin * 2 > target.Height)
            return; // Çok küçük fotoğraf, filigran sığmıyor

        var location = new Point(target.Width - scaled.Width - margin, target.Height - scaled.Height - margin);
        target.Mutate(x => x.DrawImage(scaled, location, WatermarkOpacity));
    }

    private async Task<VehicleMediaEntity> ProcessImageAsync(Guid vehicleId, IFormFile file, Image watermark, List<string> writtenPublic, List<string> writtenPrivate)
    {
        var mediaId = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        await using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer);

        Image image;
        try
        {
            buffer.Position = 0;
            image = await Image.LoadAsync(buffer);
        }
        catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException or NotSupportedException)
        {
            _logger.LogWarning(ex, "Fotoğraf okunamadı: {FileName}", file.FileName);
            return null;
        }

        using (image)
        {
            PrepareImage(image);

            var originalPath = $"originals/vehicles/{vehicleId:N}/{mediaId:N}{extension}";
            buffer.Position = 0;
            await _storage.SavePrivateAsync(originalPath, buffer);
            writtenPrivate.Add(originalPath);

            var entity = new VehicleMediaEntity
            {
                Id = mediaId,
                VehicleId = vehicleId,
                OriginalPath = originalPath,
                Width = image.Width,
                Height = image.Height
            };

            (entity.SmallPath, entity.MediumPath, entity.LargePath) =
                await WriteVariantsAsync(image, vehicleId, mediaId, watermark, writtenPublic);

            return entity;
        }
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
