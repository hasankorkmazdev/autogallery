using FatihTasci.Core.Entity.Crm;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Common;
using FatihTasci.Core.Models.Domain.Delivery;
using FatihTasci.Core.Models.Domain.Public;
using FatihTasci.Core.Providers;
using FatihTasci.Core.RepositoryBase;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace FatihTasci.Core.Service.Domains;

/// <summary>
/// Teslimatlar: müşteriye teslim edilen aracın fotoğrafı, araç adı, isteğe bağlı müşteri adı ve şehir, teslim tarihi.
/// Sıra teslim tarihidir (en yeni önce). Fotoğraf eklemede zorunludur; yeni fotoğraf eskisinin yerine geçer.
/// Orijinal özel klasörde, EXIF temizlenmiş 800 / 1600 px WebP varyantları /media/deliveries altında saklanır (filigran yok).
/// Video isteğe bağlıdır: MP4 olduğu gibi (en çok 100 MB), afiş panelin videodan aldığı ilk kare (1600 px WebP).
/// </summary>
public class DeliveryService : DomainService, IDeliveryService
{
    public const int PageSize = 24;
    public const long MaxImageBytes = 25 * 1024 * 1024;

    private const int MediumWidth = 800;
    private const int LargeWidth = 1600;
    private const int VideoPosterSize = 1600;

    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<DeliveryEntity> _deliveryRepository;
    private readonly IMediaStorage _storage;
    private readonly ILogger<DeliveryService> _logger;

    public DeliveryService(IUnitOfWork unitOfWork, IMediaStorage storage, ILogger<DeliveryService> logger)
    {
        _unitOfWork = unitOfWork;
        _deliveryRepository = unitOfWork.GetRepository<DeliveryEntity>();
        _storage = storage;
        _logger = logger;
    }

    public async Task<ServiceResultModel<List<DeliveryModel>>> GetAllAsync()
    {
        return Result(await LoadAsync());
    }

    public async Task<List<DeliveryModel>> GetLatestPublicAsync(int count)
    {
        var deliveries = await PublicQuery().Take(count).ToListAsync();
        return deliveries.Select(ToModel).ToList();
    }

    public async Task<DeliveriesPageModel> GetPublicPageAsync(int page)
    {
        var total = await PublicQuery().CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);

        var deliveries = await PublicQuery().Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();
        return new DeliveriesPageModel
        {
            Items = deliveries.Select(ToModel).ToList(),
            TotalCount = total,
            Page = page,
            TotalPages = totalPages
        };
    }

    public async Task<DateTime?> GetLastPublicChangeAsync()
    {
        return await _deliveryRepository.GetQuery()
            .Where(x => x.IsActive && x.MediumPath != null)
            .MaxAsync(x => (DateTime?)(x.LastUpdateDate ?? x.CreateDate));
    }

    public async Task<ServiceResultModel<List<DeliveryModel>>> CreateAsync(SaveDeliveryRequestModel request, IFormFile photo, IFormFile video, IFormFile videoPoster)
    {
        if (!HasFile(photo))
            return Result<List<DeliveryModel>>(null, "Teslimat fotoğrafını seçin.", 400);
        var error = ValidatePhoto(photo) ?? await ValidateVideoAsync(video, videoPoster);
        if (error != null)
            return Result<List<DeliveryModel>>(null, error, 400);

        var delivery = new DeliveryEntity();
        Apply(delivery, request);

        var written = await ProcessImageAsync(delivery, photo);
        if (written == null)
            return Result<List<DeliveryModel>>(null, PhotoUnreadableMessage, 400);

        if (HasFile(video) && !await AddVideoAsync(delivery, video, videoPoster, written))
            return Result<List<DeliveryModel>>(null, VideoUpload.PosterUnreadableMessage, 400);

        _deliveryRepository.Insert(delivery);
        await SaveOrCleanupAsync(written);

        return Result(await LoadAsync(), "Teslimat eklendi.");
    }

    public async Task<ServiceResultModel<List<DeliveryModel>>> UpdateAsync(int id, SaveDeliveryRequestModel request, IFormFile photo, IFormFile video, IFormFile videoPoster)
    {
        var delivery = await _deliveryRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == id, disableTracking: false);
        if (delivery == null)
            return Result<List<DeliveryModel>>(null, "Teslimat bulunamadı.", 404);

        var hasPhoto = HasFile(photo);
        var hasVideo = HasFile(video);
        var error = (hasPhoto ? ValidatePhoto(photo) : null) ?? await ValidateVideoAsync(video, videoPoster);
        if (error != null)
            return Result<List<DeliveryModel>>(null, error, 400);

        Apply(delivery, request);

        // Yeni dosyalar yazılır; kayıt kaydedilince değiştirilen ya da kaldırılan eski dosyalar silinir.
        var written = new List<(bool IsPublic, string Path)>();
        var oldPaths = new List<(bool IsPublic, string Path)>();

        if (hasPhoto)
        {
            var oldPhoto = PhotoPaths(delivery);
            var photoWritten = await ProcessImageAsync(delivery, photo);
            if (photoWritten == null)
                return Result<List<DeliveryModel>>(null, PhotoUnreadableMessage, 400);
            written.AddRange(photoWritten);
            oldPaths.AddRange(oldPhoto);
        }

        if (hasVideo)
        {
            var oldVideo = VideoPaths(delivery);
            if (!await AddVideoAsync(delivery, video, videoPoster, written))
                return Result<List<DeliveryModel>>(null, VideoUpload.PosterUnreadableMessage, 400);
            oldPaths.AddRange(oldVideo);
        }
        else if (request.RemoveVideo)
        {
            oldPaths.AddRange(VideoPaths(delivery));
            delivery.VideoPath = null;
            delivery.VideoPosterPath = null;
            delivery.VideoWidth = null;
            delivery.VideoHeight = null;
        }

        await SaveOrCleanupAsync(written);
        Cleanup(oldPaths);

        return Result(await LoadAsync(), "Teslimat güncellendi.");
    }

    public async Task<ServiceResultModel<List<DeliveryModel>>> DeleteAsync(int id)
    {
        var delivery = await _deliveryRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == id, disableTracking: false);
        if (delivery == null)
            return Result<List<DeliveryModel>>(null, "Teslimat bulunamadı.", 404);

        var paths = PhotoPaths(delivery).Concat(VideoPaths(delivery)).ToList();
        _deliveryRepository.Delete(delivery);
        await _unitOfWork.SaveChangesAsync();
        Cleanup(paths);

        return Result(await LoadAsync(), "Teslimat silindi.");
    }

    private static void Apply(DeliveryEntity delivery, SaveDeliveryRequestModel request)
    {
        delivery.VehicleTitle = request.VehicleTitle.Trim();
        delivery.CustomerName = string.IsNullOrWhiteSpace(request.CustomerName) ? null : request.CustomerName.Trim();
        delivery.City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim();
        delivery.DeliveredAt = request.DeliveredAt!.Value.Date;
        delivery.IsActive = request.IsActive;
    }

    private const string PhotoUnreadableMessage = "Dosya okunamadı, geçerli bir fotoğraf değil.";

    private static bool HasFile(IFormFile file) => file != null && file.Length > 0;

    /// <summary>
    /// Video gönderilmediyse null (video isteğe bağlıdır), gönderildiyse VideoUpload denetimi.
    /// </summary>
    private static async Task<string> ValidateVideoAsync(IFormFile video, IFormFile poster)
    {
        return HasFile(video) ? await VideoUpload.ValidateAsync(video, poster) : null;
    }

    private static string ValidatePhoto(IFormFile photo)
    {
        if (!AllowedImageExtensions.Contains(Path.GetExtension(photo.FileName).ToLowerInvariant()))
            return "Yalnızca JPG, PNG ve WebP yüklenebilir.";
        if (photo.Length > MaxImageBytes)
            return "Fotoğraf en fazla 25 MB olabilir.";
        return null;
    }

    private async Task SaveOrCleanupAsync(List<(bool IsPublic, string Path)> written)
    {
        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            Cleanup(written);
            throw;
        }
    }

    /// <summary>
    /// Videoyu olduğu gibi, afişini (panelin videodan aldığı ilk kare) 1600 px WebP olarak yazar ve yolları entity'ye
    /// işler; yazdıklarını written'a ekler. Afiş okunamazsa written'daki dosyaları siler ve false döner.
    /// </summary>
    private async Task<bool> AddVideoAsync(DeliveryEntity delivery, IFormFile video, IFormFile poster, List<(bool IsPublic, string Path)> written)
    {
        using var posterImage = VideoUpload.DecodePoster(poster, VideoPosterSize, out var width, out var height);
        if (posterImage == null)
        {
            _logger.LogWarning("Teslimat videosunun afişi okunamadı: {FileName}", video.FileName);
            Cleanup(written);
            return false;
        }

        var basePath = $"deliveries/video-{Guid.NewGuid():N}";
        var posterPath = $"{basePath}-poster-{VideoPosterSize}.webp";
        var videoPath = $"{basePath}.mp4";
        try
        {
            await VideoUpload.SavePosterAsync(_storage, posterImage, posterPath, VideoPosterSize);
            written.Add((true, posterPath));

            await using (var stream = video.OpenReadStream())
                await _storage.SavePublicAsync(videoPath, stream);
            written.Add((true, videoPath));
        }
        catch
        {
            Cleanup(written);
            throw;
        }

        delivery.VideoPath = videoPath;
        delivery.VideoPosterPath = posterPath;
        delivery.VideoWidth = width;
        delivery.VideoHeight = height;
        return true;
    }

    private async Task<List<(bool IsPublic, string Path)>> ProcessImageAsync(DeliveryEntity delivery, IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var written = await WriteImagesAsync(_storage, delivery, stream, Path.GetExtension(file.FileName), _logger);
        if (written == null)
            _logger.LogWarning("Teslimat fotoğrafı okunamadı: {FileName}", file.FileName);
        return written;
    }

    /// <summary>
    /// Fotoğrafı okur (yön uygulanır, yeniden kodlanan varyantlarda metadata kalmaz); orijinali ve 800 / 1600 px WebP
    /// varyantlarını yazar, yolları entity'ye işler. Panel yüklemesi ve geliştirme seed'i (DbSeeder) aynı yolu kullanır.
    /// Okunamayan dosyada null döner. Dönen liste yazılan dosyalardır (kaydetme başarısız olursa çağıran siler).
    /// </summary>
    public static async Task<List<(bool IsPublic, string Path)>> WriteImagesAsync(
        IMediaStorage storage, DeliveryEntity delivery, Stream source, string extension, ILogger logger = null)
    {
        await using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer);

        buffer.Position = 0;
        using var image = ImageTools.Decode(buffer, LargeWidth);
        if (image == null)
            return null;

        var written = new List<(bool IsPublic, string Path)>();
        var fileId = Guid.NewGuid().ToString("N");
        try
        {
            var originalPath = $"originals/deliveries/{fileId}{extension.ToLowerInvariant()}";
            buffer.Position = 0;
            await storage.SavePrivateAsync(originalPath, buffer);
            written.Add((false, originalPath));

            var mediumPath = $"deliveries/{fileId}-{MediumWidth}.webp";
            await SaveVariantAsync(storage, image, MediumWidth, 80, mediumPath);
            written.Add((true, mediumPath));

            var largePath = $"deliveries/{fileId}-{LargeWidth}.webp";
            await SaveVariantAsync(storage, image, LargeWidth, 82, largePath);
            written.Add((true, largePath));

            delivery.OriginalPath = originalPath;
            delivery.MediumPath = mediumPath;
            delivery.LargePath = largePath;
        }
        catch
        {
            Cleanup(storage, written, logger);
            throw;
        }

        return written;
    }

    private static async Task SaveVariantAsync(IMediaStorage storage, SKBitmap image, int width, int quality, string path)
    {
        using var variant = ImageTools.ScaleToWidth(image, width);
        await using var output = ImageTools.ToWebp(variant, quality);
        await storage.SavePublicAsync(path, output);
    }

    private IQueryable<DeliveryEntity> PublicQuery()
    {
        return _deliveryRepository.GetQuery()
            .Where(x => x.IsActive && x.MediumPath != null && x.LargePath != null)
            .OrderByDescending(x => x.DeliveredAt).ThenByDescending(x => x.Id);
    }

    private async Task<List<DeliveryModel>> LoadAsync()
    {
        var deliveries = await _deliveryRepository.GetQuery()
            .OrderByDescending(x => x.DeliveredAt).ThenByDescending(x => x.Id)
            .ToListAsync();
        return deliveries.Select(ToModel).ToList();
    }

    private DeliveryModel ToModel(DeliveryEntity x) => new()
    {
        Id = x.Id,
        VehicleTitle = x.VehicleTitle,
        CustomerName = x.CustomerName,
        City = x.City,
        DeliveredAt = x.DeliveredAt,
        IsActive = x.IsActive,
        MediumUrl = _storage.ToUrl(x.MediumPath),
        LargeUrl = _storage.ToUrl(x.LargePath),
        Video = string.IsNullOrEmpty(x.VideoPath)
            ? null
            : new VideoFileModel
            {
                Url = _storage.ToUrl(x.VideoPath),
                PosterUrl = _storage.ToUrl(x.VideoPosterPath),
                Width = x.VideoWidth ?? 0,
                Height = x.VideoHeight ?? 0
            }
    };

    private static List<(bool IsPublic, string Path)> PhotoPaths(DeliveryEntity delivery)
    {
        return new List<(bool IsPublic, string Path)>
        {
            (false, delivery.OriginalPath),
            (true, delivery.MediumPath),
            (true, delivery.LargePath)
        };
    }

    private static List<(bool IsPublic, string Path)> VideoPaths(DeliveryEntity delivery)
    {
        return new List<(bool IsPublic, string Path)>
        {
            (true, delivery.VideoPath),
            (true, delivery.VideoPosterPath)
        };
    }

    private void Cleanup(IEnumerable<(bool IsPublic, string Path)> paths)
    {
        Cleanup(_storage, paths, _logger);
    }

    private static void Cleanup(IMediaStorage storage, IEnumerable<(bool IsPublic, string Path)> paths, ILogger logger)
    {
        foreach (var (isPublic, path) in paths)
        {
            if (string.IsNullOrEmpty(path))
                continue;
            try
            {
                if (isPublic)
                    storage.DeletePublic(path);
                else
                    storage.DeletePrivate(path);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Dosya silinemedi: {Path}", path);
            }
        }
    }
}
