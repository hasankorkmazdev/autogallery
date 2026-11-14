using FatihTasci.Core.Entity.Common;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Setting;
using FatihTasci.Core.Providers;
using FatihTasci.Core.RepositoryBase;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Reflection;

namespace FatihTasci.Core.Service.Domains;

/// <summary>
/// SiteSettingsModel'in her yazılabilir özelliği SiteSettings tablosunda aynı adlı bir anahtardır
/// (bool değerler "true" / "false" olarak saklanır). LogoPath formla değil logo uçlarıyla değişir.
/// Vitrin her istekte okuduğu için sonuç bellekte tutulur, güncellemede temizlenir.
/// </summary>
public class SiteSettingService : DomainService, ISiteSettingService
{
    public const long MaxLogoBytes = 5 * 1024 * 1024;
    private const int MaxLogoSize = 1200;
    private const string CacheKey = "site-settings";
    private static readonly string[] AllowedLogoExtensions = { ".png", ".webp", ".jpg", ".jpeg" };
    private static readonly PropertyInfo[] Properties = typeof(SiteSettingsModel).GetProperties().Where(x => x.CanWrite).ToArray();

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<SiteSettingEntity> _settingRepository;
    private readonly IMemoryCache _cache;
    private readonly IMediaStorage _storage;
    private readonly ILogger<SiteSettingService> _logger;

    public SiteSettingService(IUnitOfWork unitOfWork, IMemoryCache cache, IMediaStorage storage, ILogger<SiteSettingService> logger)
    {
        _unitOfWork = unitOfWork;
        _settingRepository = unitOfWork.GetRepository<SiteSettingEntity>();
        _cache = cache;
        _storage = storage;
        _logger = logger;
    }

    public async Task<SiteSettingsModel> GetAsync()
    {
        if (_cache.TryGetValue(CacheKey, out SiteSettingsModel cached))
            return cached;

        var values = await _settingRepository.GetQuery().ToDictionaryAsync(x => x.Key, x => x.Value);
        var model = new SiteSettingsModel();
        foreach (var property in Properties)
        {
            if (!values.TryGetValue(property.Name, out var value))
                continue;

            if (property.PropertyType == typeof(bool))
            {
                if (bool.TryParse(value, out var flag))
                    property.SetValue(model, flag);
            }
            else
            {
                property.SetValue(model, value);
            }
        }

        _cache.Set(CacheKey, model, TimeSpan.FromMinutes(10));
        return model;
    }

    public async Task<ServiceResultModel<SiteSettingsModel>> UpdateAsync(SiteSettingsModel request)
    {
        var existing = await _settingRepository.GetQuery(disableTracking: false).ToListAsync();

        foreach (var property in Properties)
        {
            if (property.Name == nameof(SiteSettingsModel.LogoPath))
                continue;

            SetValue(existing, property.Name, ToStoredValue(property.GetValue(request)));
        }

        await _unitOfWork.SaveChangesAsync();
        _cache.Remove(CacheKey);

        return Result(await GetAsync(), "Ayarlar kaydedildi.");
    }

    public async Task<ServiceResultModel<SiteSettingsModel>> UploadLogoAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Result<SiteSettingsModel>(null, "Yüklenecek logo dosyasını seçin.", 400);
        if (file.Length > MaxLogoBytes)
            return Result<SiteSettingsModel>(null, "Logo en fazla 5 MB olabilir.", 400);
        if (!AllowedLogoExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
            return Result<SiteSettingsModel>(null, "Logo PNG, WebP ya da JPG olmalıdır. Filigran için saydam zeminli PNG önerilir.", 400);

        Image image;
        try
        {
            await using var input = file.OpenReadStream();
            image = await Image.LoadAsync(input);
        }
        catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException or NotSupportedException)
        {
            _logger.LogWarning(ex, "Logo okunamadı: {FileName}", file.FileName);
            return Result<SiteSettingsModel>(null, "Dosya okunamadı, geçerli bir görsel değil.", 400);
        }

        // Saydamlık korunsun diye PNG olarak saklanır; adı her yüklemede değişir (medya uzun süre önbelleğe alınıyor).
        var path = $"site/logo-{Guid.NewGuid():N}.png";
        using (image)
        {
            image.Mutate(x => x.AutoOrient());
            image.Metadata.ExifProfile = null;
            image.Metadata.XmpProfile = null;
            image.Metadata.IptcProfile = null;
            if (image.Width > MaxLogoSize || image.Height > MaxLogoSize)
                image.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(MaxLogoSize, MaxLogoSize), Mode = ResizeMode.Max }));

            await using var output = new MemoryStream();
            await image.SaveAsPngAsync(output);
            output.Position = 0;
            await _storage.SavePublicAsync(path, output);
        }

        var oldPath = await ReplaceLogoPathAsync(path);
        TryDeletePublic(oldPath);

        return Result(await GetAsync(), "Logo yüklendi. Mevcut fotoğraflara uygulamak için \"Filigranı yeniden uygula\"yı kullanın.");
    }

    public async Task<ServiceResultModel<SiteSettingsModel>> DeleteLogoAsync()
    {
        var oldPath = await ReplaceLogoPathAsync(null);
        TryDeletePublic(oldPath);

        return Result(await GetAsync(), "Logo kaldırıldı.");
    }

    private async Task<string> ReplaceLogoPathAsync(string path)
    {
        var existing = await _settingRepository.GetQuery(disableTracking: false).ToListAsync();
        var oldPath = existing.FirstOrDefault(x => x.Key == nameof(SiteSettingsModel.LogoPath))?.Value;

        SetValue(existing, nameof(SiteSettingsModel.LogoPath), path);
        await _unitOfWork.SaveChangesAsync();
        _cache.Remove(CacheKey);

        return oldPath;
    }

    private void SetValue(List<SiteSettingEntity> existing, string key, string value)
    {
        var entity = existing.FirstOrDefault(x => x.Key == key);
        if (entity == null)
            _settingRepository.Insert(new SiteSettingEntity { Key = key, Value = value });
        else
            entity.Value = value;
    }

    private static string ToStoredValue(object value)
    {
        if (value is bool flag)
            return flag ? "true" : "false";

        var text = (value as string)?.Trim();
        return string.IsNullOrEmpty(text) ? null : text;
    }

    private void TryDeletePublic(string path)
    {
        try
        {
            _storage.DeletePublic(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Eski logo silinemedi: {Path}", path);
        }
    }
}
