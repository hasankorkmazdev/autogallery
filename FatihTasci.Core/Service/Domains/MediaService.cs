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
/// İlk sürümde işleme yükleme isteği içinde yapılır (UYGULAMA_PLANI: yük artarsa Hangfire'a taşınır).
/// </summary>
public class MediaService : DomainService, IMediaService
{
    public const int MaxPhotosPerVehicle = 60;
    public const long MaxImageBytes = 25 * 1024 * 1024;
    public const long MaxPdfBytes = 20 * 1024 * 1024;

    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly (string Suffix, int Width, int Quality)[] Variants =
    {
        ("480", 480, 75),
        ("1280", 1280, 80),
        ("2400", 2400, 82)
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<VehicleEntity> _vehicleRepository;
    private readonly IRepository<VehicleMediaEntity> _mediaRepository;
    private readonly IMediaStorage _storage;
    private readonly ILogger<MediaService> _logger;

    public MediaService(IUnitOfWork unitOfWork, IMediaStorage storage, ILogger<MediaService> logger)
    {
        _unitOfWork = unitOfWork;
        _vehicleRepository = unitOfWork.GetRepository<VehicleEntity>();
        _mediaRepository = unitOfWork.GetRepository<VehicleMediaEntity>();
        _storage = storage;
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

        var existingCount = await _mediaRepository.CountAsync(x => x.VehicleId == vehicleId);
        if (existingCount + files.Count > MaxPhotosPerVehicle)
            return Result<List<VehicleMediaModel>>(null, $"Bir araca en fazla {MaxPhotosPerVehicle} fotoğraf yüklenebilir.", 400);

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

        try
        {
            foreach (var file in files)
            {
                var media = await ProcessImageAsync(vehicleId, file, writtenPublic, writtenPrivate);
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

    private async Task<VehicleMediaEntity> ProcessImageAsync(Guid vehicleId, IFormFile file, List<string> writtenPublic, List<string> writtenPrivate)
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
            // Telefon fotoğraflarındaki yönü uygula, ardından konum dahil tüm metadata'yı temizle.
            image.Mutate(x => x.AutoOrient());
            image.Metadata.ExifProfile = null;
            image.Metadata.XmpProfile = null;
            image.Metadata.IptcProfile = null;

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

            foreach (var (suffix, width, quality) in Variants)
            {
                var path = $"vehicles/{vehicleId:N}/{mediaId:N}-{suffix}.webp";
                using var variant = image.Width > width
                    ? image.Clone(x => x.Resize(new ResizeOptions { Size = new Size(width, 0), Mode = ResizeMode.Max }))
                    : image.Clone(_ => { });

                await using var output = new MemoryStream();
                await variant.SaveAsWebpAsync(output, new WebpEncoder { Quality = quality });
                output.Position = 0;
                await _storage.SavePublicAsync(path, output);
                writtenPublic.Add(path);

                switch (suffix)
                {
                    case "480": entity.SmallPath = path; break;
                    case "1280": entity.MediumPath = path; break;
                    default: entity.LargePath = path; break;
                }
            }

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
