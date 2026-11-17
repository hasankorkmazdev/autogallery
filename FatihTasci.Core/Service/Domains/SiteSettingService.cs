using FatihTasci.Core.Entity.Common;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Setting;
using FatihTasci.Core.Providers;
using FatihTasci.Core.RepositoryBase;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;

namespace FatihTasci.Core.Service.Domains;

/// <summary>
/// SiteSettingsModel'in her yazılabilir özelliği SiteSettings tablosunda aynı adlı bir anahtardır
/// (bool değerler "true" / "false", listeler JSON olarak saklanır). LogoPath formla değil logo uçlarıyla değişir.
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
            else if (property.PropertyType == typeof(List<SocialLinkModel>))
            {
                property.SetValue(model, ReadSocialLinks(value));
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

        await using var input = file.OpenReadStream();
        using var image = ImageTools.Decode(input, MaxLogoSize, MaxLogoSize);
        if (image == null)
        {
            _logger.LogWarning("Logo okunamadı: {FileName}", file.FileName);
            return Result<SiteSettingsModel>(null, "Dosya okunamadı, geçerli bir görsel değil.", 400);
        }

        // Saydamlık korunsun diye PNG olarak saklanır; adı her yüklemede değişir (medya uzun süre önbelleğe alınıyor).
        var path = $"site/logo-{Guid.NewGuid():N}.png";
        using (var resized = ImageTools.Fit(image, MaxLogoSize, MaxLogoSize))
        await using (var output = ImageTools.ToPng(resized))
        {
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

        if (value is List<SocialLinkModel> links)
        {
            var cleaned = links
                .Where(x => !string.IsNullOrWhiteSpace(x.Url))
                .Select(x => new SocialLinkModel { Platform = x.Platform, Url = x.Url.Trim() })
                .ToList();
            return cleaned.Count == 0 ? null : JsonSerializer.Serialize(cleaned);
        }

        var text = (value as string)?.Trim();
        return string.IsNullOrEmpty(text) ? null : text;
    }

    private List<SocialLinkModel> ReadSocialLinks(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new List<SocialLinkModel>();

        try
        {
            return JsonSerializer.Deserialize<List<SocialLinkModel>>(value) ?? new List<SocialLinkModel>();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Sosyal medya bağlantıları okunamadı.");
            return new List<SocialLinkModel>();
        }
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
