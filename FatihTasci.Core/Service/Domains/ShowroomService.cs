using FatihTasci.Core.Entity.Common;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Showroom;
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
/// Ana sayfa "Galeri Atmosferi" sahneleri. Her sahnenin tek fotoğrafı olur; yeni fotoğraf eskisinin yerine geçer.
/// Orijinal dosya özel klasörde, EXIF temizlenmiş 1280 / 2400 px WebP varyantları /media/showroom altında saklanır.
/// </summary>
public class ShowroomService : DomainService, IShowroomService
{
    public const int MaxScenes = 8;
    public const long MaxImageBytes = 25 * 1024 * 1024;

    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<ShowroomSceneEntity> _sceneRepository;
    private readonly IMediaStorage _storage;
    private readonly ILogger<ShowroomService> _logger;

    public ShowroomService(IUnitOfWork unitOfWork, IMediaStorage storage, ILogger<ShowroomService> logger)
    {
        _unitOfWork = unitOfWork;
        _sceneRepository = unitOfWork.GetRepository<ShowroomSceneEntity>();
        _storage = storage;
        _logger = logger;
    }

    public async Task<ServiceResultModel<List<ShowroomSceneModel>>> GetAllAsync()
    {
        return Result(await LoadAsync());
    }

    public async Task<List<ShowroomSceneModel>> GetPublicAsync()
    {
        var scenes = await _sceneRepository.GetQuery()
            .Where(x => x.IsActive && x.MediumPath != null && x.LargePath != null)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .ToListAsync();
        return scenes.Select(ToModel).ToList();
    }

    public async Task<ServiceResultModel<List<ShowroomSceneModel>>> CreateAsync(SaveShowroomSceneRequestModel request)
    {
        var count = await _sceneRepository.CountAsync();
        if (count >= MaxScenes)
            return Result<List<ShowroomSceneModel>>(null, $"En fazla {MaxScenes} sahne eklenebilir.", 400);

        var nextOrder = count == 0 ? 0 : await _sceneRepository.GetQuery().MaxAsync(x => x.SortOrder) + 1;
        _sceneRepository.Insert(new ShowroomSceneEntity
        {
            Tag = request.Tag.Trim(),
            Title = request.Title.Trim(),
            IsActive = request.IsActive,
            SortOrder = nextOrder
        });
        await _unitOfWork.SaveChangesAsync();

        return Result(await LoadAsync(), "Sahne eklendi.");
    }

    public async Task<ServiceResultModel<List<ShowroomSceneModel>>> UpdateAsync(int id, SaveShowroomSceneRequestModel request)
    {
        var scene = await _sceneRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == id, disableTracking: false);
        if (scene == null)
            return Result<List<ShowroomSceneModel>>(null, "Sahne bulunamadı.", 404);

        scene.Tag = request.Tag.Trim();
        scene.Title = request.Title.Trim();
        scene.IsActive = request.IsActive;
        await _unitOfWork.SaveChangesAsync();

        return Result(await LoadAsync(), "Sahne güncellendi.");
    }

    public async Task<ServiceResultModel<List<ShowroomSceneModel>>> ReorderAsync(ShowroomSceneOrderRequestModel request)
    {
        var scenes = await _sceneRepository.GetQuery(disableTracking: false).ToListAsync();
        if (scenes.Count != request.SceneIds.Count || scenes.Any(x => !request.SceneIds.Contains(x.Id)))
            return Result<List<ShowroomSceneModel>>(null, "Sahne listesi güncel değil, sayfayı yenileyin.", 400);

        foreach (var scene in scenes)
            scene.SortOrder = request.SceneIds.IndexOf(scene.Id);

        await _unitOfWork.SaveChangesAsync();
        return Result(await LoadAsync(), "Sıralama kaydedildi.");
    }

    public async Task<ServiceResultModel<List<ShowroomSceneModel>>> DeleteAsync(int id)
    {
        var scene = await _sceneRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == id, disableTracking: false);
        if (scene == null)
            return Result<List<ShowroomSceneModel>>(null, "Sahne bulunamadı.", 404);

        var paths = Paths(scene);
        _sceneRepository.Delete(scene);
        await _unitOfWork.SaveChangesAsync();
        Cleanup(paths);

        return Result(await LoadAsync(), "Sahne silindi.");
    }

    public async Task<ServiceResultModel<List<ShowroomSceneModel>>> UploadImageAsync(int id, IFormFile file)
    {
        var scene = await _sceneRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == id, disableTracking: false);
        if (scene == null)
            return Result<List<ShowroomSceneModel>>(null, "Sahne bulunamadı.", 404);

        if (file == null || file.Length == 0)
            return Result<List<ShowroomSceneModel>>(null, "Yüklenecek fotoğrafı seçin.", 400);
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
            return Result<List<ShowroomSceneModel>>(null, "Yalnızca JPG, PNG ve WebP yüklenebilir.", 400);
        if (file.Length > MaxImageBytes)
            return Result<List<ShowroomSceneModel>>(null, "Fotoğraf en fazla 25 MB olabilir.", 400);

        var oldPaths = Paths(scene);
        var written = await ProcessImageAsync(scene, file, extension);
        if (written == null)
            return Result<List<ShowroomSceneModel>>(null, "Dosya okunamadı, geçerli bir fotoğraf değil.", 400);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            Cleanup(written);
            throw;
        }

        Cleanup(oldPaths);
        return Result(await LoadAsync(), "Fotoğraf yüklendi.");
    }

    public async Task<ServiceResultModel<List<ShowroomSceneModel>>> DeleteImageAsync(int id)
    {
        var scene = await _sceneRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == id, disableTracking: false);
        if (scene == null)
            return Result<List<ShowroomSceneModel>>(null, "Sahne bulunamadı.", 404);

        var paths = Paths(scene);
        scene.OriginalPath = null;
        scene.MediumPath = null;
        scene.LargePath = null;
        await _unitOfWork.SaveChangesAsync();
        Cleanup(paths);

        return Result(await LoadAsync(), "Fotoğraf kaldırıldı. Sahne fotoğraf yüklenene kadar sitede görünmez.");
    }

    /// <summary>
    /// Fotoğrafı okur, yönünü uygular, metadata'yı temizler; orijinali ve WebP varyantlarını yazar, yolları entity'ye işler.
    /// Okunamayan dosyada null döner. Dönen liste yazılan dosyalardır (kaydetme başarısız olursa silinir).
    /// </summary>
    private async Task<List<(bool IsPublic, string Path)>> ProcessImageAsync(ShowroomSceneEntity scene, IFormFile file, string extension)
    {
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
            _logger.LogWarning(ex, "Showroom fotoğrafı okunamadı: {FileName}", file.FileName);
            return null;
        }

        var written = new List<(bool IsPublic, string Path)>();
        var fileId = Guid.NewGuid().ToString("N");
        try
        {
            using (image)
            {
                image.Mutate(x => x.AutoOrient());
                image.Metadata.ExifProfile = null;
                image.Metadata.XmpProfile = null;
                image.Metadata.IptcProfile = null;

                var originalPath = $"originals/showroom/{fileId}{extension}";
                buffer.Position = 0;
                await _storage.SavePrivateAsync(originalPath, buffer);
                written.Add((false, originalPath));

                var mediumPath = $"showroom/{fileId}-1280.webp";
                await SaveVariantAsync(image, 1280, 80, mediumPath);
                written.Add((true, mediumPath));

                var largePath = $"showroom/{fileId}-2400.webp";
                await SaveVariantAsync(image, 2400, 82, largePath);
                written.Add((true, largePath));

                scene.OriginalPath = originalPath;
                scene.MediumPath = mediumPath;
                scene.LargePath = largePath;
            }
        }
        catch
        {
            Cleanup(written);
            throw;
        }

        return written;
    }

    private async Task SaveVariantAsync(Image image, int width, int quality, string path)
    {
        using var variant = image.Width > width
            ? image.Clone(x => x.Resize(new ResizeOptions { Size = new Size(width, 0), Mode = ResizeMode.Max }))
            : image.Clone(_ => { });

        await using var output = new MemoryStream();
        await variant.SaveAsWebpAsync(output, new WebpEncoder { Quality = quality });
        output.Position = 0;
        await _storage.SavePublicAsync(path, output);
    }

    private async Task<List<ShowroomSceneModel>> LoadAsync()
    {
        var scenes = await _sceneRepository.GetQuery()
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .ToListAsync();
        return scenes.Select(ToModel).ToList();
    }

    private ShowroomSceneModel ToModel(ShowroomSceneEntity x) => new()
    {
        Id = x.Id,
        Tag = x.Tag,
        Title = x.Title,
        SortOrder = x.SortOrder,
        IsActive = x.IsActive,
        MediumUrl = _storage.ToUrl(x.MediumPath),
        LargeUrl = _storage.ToUrl(x.LargePath)
    };

    private static List<(bool IsPublic, string Path)> Paths(ShowroomSceneEntity scene)
    {
        return new List<(bool IsPublic, string Path)>
        {
            (false, scene.OriginalPath),
            (true, scene.MediumPath),
            (true, scene.LargePath)
        };
    }

    private void Cleanup(IEnumerable<(bool IsPublic, string Path)> paths)
    {
        foreach (var (isPublic, path) in paths)
        {
            if (string.IsNullOrEmpty(path))
                continue;
            try
            {
                if (isPublic)
                    _storage.DeletePublic(path);
                else
                    _storage.DeletePrivate(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Dosya silinemedi: {Path}", path);
            }
        }
    }
}
