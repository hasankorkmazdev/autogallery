using FatihTasci.Core.Entity.Common;
using FatihTasci.Core.Enums;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Content;
using FatihTasci.Core.Providers;
using FatihTasci.Core.RepositoryBase;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System.Text.Json;

namespace FatihTasci.Core.Service.Domains;

/// <summary>
/// Vitrin yazıları SiteSettings tablosunda "Content." önekli anahtarlarda saklanır (her metin ayrı anahtar, değer
/// sütunu 4000 karakter). Anahtar hiç yoksa SiteContentDefaults'taki metin gelir; boş kaydedilen metin boş kalır.
/// Vitrin her istekte okuduğu için sonuç bellekte tutulur, güncellemede temizlenir.
/// Ana sayfa videosu "Content.HeroVideo" anahtarında dosya kökü olarak saklanır (site/hero-{guid}: .mp4 ve iki afiş);
/// boşsa varsayılan drone videosu kullanılır. /media bir yıl önbelleğe alındığı için her yüklemede yeni ad verilir.
/// </summary>
public class SiteContentService : DomainService, ISiteContentService
{
    private const int PosterWidth = 1920;
    private const int PosterSmallWidth = 960;
    private const int PosterQuality = 80;
    private const string CacheKey = "site-content";
    private const string KeyPrefix = "Content.";
    private const string StatsKey = KeyPrefix + "Stats";
    private const string CorporateIntroKey = KeyPrefix + "Corporate.Intro";
    private const string CorporateAboutKey = KeyPrefix + "Corporate.About";
    private const string CorporateVisionKey = KeyPrefix + "Corporate.Vision";
    private const string CorporateMissionKey = KeyPrefix + "Corporate.Mission";
    private const string HeroVideoKey = KeyPrefix + "HeroVideo";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<SiteSettingEntity> _settingRepository;
    private readonly IMemoryCache _cache;
    private readonly IMediaStorage _storage;
    private readonly ILogger<SiteContentService> _logger;

    public SiteContentService(IUnitOfWork unitOfWork, IMemoryCache cache, IMediaStorage storage, ILogger<SiteContentService> logger)
    {
        _unitOfWork = unitOfWork;
        _settingRepository = unitOfWork.GetRepository<SiteSettingEntity>();
        _cache = cache;
        _storage = storage;
        _logger = logger;
    }

    private static string HeroVideoPath(string basePath) => basePath + ".mp4";
    private static string HeroPosterPath(string basePath) => basePath + "-poster-1920.webp";
    private static string HeroPosterSmallPath(string basePath) => basePath + "-poster-960.webp";

    private static string SummaryKey(SiteServiceType type) => $"{KeyPrefix}Service.{type}.Summary";
    private static string BodyKey(SiteServiceType type) => $"{KeyPrefix}Service.{type}.Body";

    public async Task<SiteContentModel> GetAsync()
    {
        if (_cache.TryGetValue(CacheKey, out SiteContentModel cached))
            return cached;

        var values = await _settingRepository.GetQuery()
            .Where(x => x.Key.StartsWith(KeyPrefix))
            .ToDictionaryAsync(x => x.Key, x => x.Value);

        string Read(string key, string fallback) => values.TryGetValue(key, out var value) ? value ?? string.Empty : fallback;

        var model = new SiteContentModel
        {
            Services = Enum.GetValues<SiteServiceType>()
                .Select(type => new ServiceContentModel
                {
                    Type = type,
                    Summary = Read(SummaryKey(type), SiteContentDefaults.ServiceSummary(type)),
                    Body = Read(BodyKey(type), SiteContentDefaults.ServiceBody(type))
                })
                .ToList(),
            CorporateIntro = Read(CorporateIntroKey, SiteContentDefaults.CorporateIntro),
            CorporateAbout = Read(CorporateAboutKey, SiteContentDefaults.CorporateAbout),
            CorporateVision = Read(CorporateVisionKey, SiteContentDefaults.CorporateVision),
            CorporateMission = Read(CorporateMissionKey, SiteContentDefaults.CorporateMission),
            Stats = values.TryGetValue(StatsKey, out var stats) ? ReadStats(stats) : SiteContentDefaults.Stats(),
            HeroVideo = BuildHeroVideo(values.TryGetValue(HeroVideoKey, out var hero) ? hero : null)
        };

        _cache.Set(CacheKey, model, TimeSpan.FromMinutes(10));
        return model;
    }

    public async Task<ServiceResultModel<SiteContentModel>> UpdateAsync(SiteContentModel request)
    {
        var existing = await _settingRepository.GetQuery(disableTracking: false)
            .Where(x => x.Key.StartsWith(KeyPrefix))
            .ToListAsync();

        // Gönderilmeyen hizmetin yazıları olduğu gibi kalır.
        foreach (var service in request.Services)
        {
            SetValue(existing, SummaryKey(service.Type), Clean(service.Summary));
            SetValue(existing, BodyKey(service.Type), Clean(service.Body));
        }

        SetValue(existing, CorporateIntroKey, Clean(request.CorporateIntro));
        SetValue(existing, CorporateAboutKey, Clean(request.CorporateAbout));
        SetValue(existing, CorporateVisionKey, Clean(request.CorporateVision));
        SetValue(existing, CorporateMissionKey, Clean(request.CorporateMission));

        var stats = (request.Stats ?? new List<StatItemModel>())
            .Select(x => new StatItemModel { Value = Clean(x.Value), Label = Clean(x.Label) })
            .Where(x => x.Value.Length > 0 && x.Label.Length > 0)
            .ToList();
        SetValue(existing, StatsKey, JsonSerializer.Serialize(stats));

        await _unitOfWork.SaveChangesAsync();
        _cache.Remove(CacheKey);

        return Result(await GetAsync(), "Site içeriği kaydedildi.");
    }

    /// <summary>
    /// Video olduğu gibi saklanır (sunucuda sıkıştırma yok; panel web'e uygun H.264 MP4 ister, bkz. VideoUpload). Afiş
    /// panelde tarayıcının videodan aldığı ilk karedir; burada 1920 ve 960 px WebP'ye çevrilir.
    /// </summary>
    public async Task<ServiceResultModel<SiteContentModel>> UploadHeroVideoAsync(IFormFile video, IFormFile poster)
    {
        var error = await VideoUpload.ValidateAsync(video, poster);
        if (error != null)
            return Result<SiteContentModel>(null, error, 400);

        SKBitmap posterImage;
        await using (var input = poster.OpenReadStream())
        {
            posterImage = ImageTools.Decode(input, PosterWidth);
        }
        if (posterImage == null)
        {
            _logger.LogWarning("Ana sayfa videosunun afişi okunamadı.");
            return Result<SiteContentModel>(null, VideoUpload.PosterUnreadableMessage, 400);
        }

        var basePath = $"site/hero-{Guid.NewGuid():N}";
        try
        {
            using (posterImage)
            {
                await SavePosterAsync(posterImage, HeroPosterPath(basePath), PosterWidth);
                await SavePosterAsync(posterImage, HeroPosterSmallPath(basePath), PosterSmallWidth);
            }

            await using var stream = video.OpenReadStream();
            await _storage.SavePublicAsync(HeroVideoPath(basePath), stream);
        }
        catch
        {
            DeleteHeroFiles(basePath);
            throw;
        }

        var oldBasePath = await ReplaceHeroVideoAsync(basePath);
        DeleteHeroFiles(oldBasePath);

        return Result(await GetAsync(), "Ana sayfa videosu yüklendi.");
    }

    public async Task<ServiceResultModel<SiteContentModel>> DeleteHeroVideoAsync()
    {
        var oldBasePath = await ReplaceHeroVideoAsync(string.Empty);
        DeleteHeroFiles(oldBasePath);

        return Result(await GetAsync(), "Varsayılan drone videosuna dönüldü.");
    }

    private HeroVideoModel BuildHeroVideo(string basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
            return SiteContentDefaults.HeroVideo();

        var videoUrl = _storage.ToUrl(HeroVideoPath(basePath));
        return new HeroVideoModel
        {
            VideoHdUrl = videoUrl,
            VideoSdUrl = videoUrl,
            PosterUrl = _storage.ToUrl(HeroPosterPath(basePath)),
            PosterSmallUrl = _storage.ToUrl(HeroPosterSmallPath(basePath)),
            IsCustom = true
        };
    }

    /// <summary>
    /// Yeni dosya kökünü yazar, eskisini döndürür (dosyaları çağıran siler).
    /// </summary>
    private async Task<string> ReplaceHeroVideoAsync(string basePath)
    {
        var existing = await _settingRepository.GetQuery(disableTracking: false)
            .Where(x => x.Key == HeroVideoKey)
            .ToListAsync();
        var oldBasePath = existing.FirstOrDefault()?.Value;

        SetValue(existing, HeroVideoKey, basePath);
        await _unitOfWork.SaveChangesAsync();
        _cache.Remove(CacheKey);

        return oldBasePath;
    }

    private async Task SavePosterAsync(SKBitmap image, string path, int width)
    {
        using var variant = ImageTools.ScaleToWidth(image, width);
        await using var output = ImageTools.ToWebp(variant, PosterQuality);
        await _storage.SavePublicAsync(path, output);
    }

    private void DeleteHeroFiles(string basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
            return;

        foreach (var path in new[] { HeroVideoPath(basePath), HeroPosterPath(basePath), HeroPosterSmallPath(basePath) })
        {
            try
            {
                _storage.DeletePublic(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Eski ana sayfa videosu dosyası silinemedi: {Path}", path);
            }
        }
    }

    private void SetValue(List<SiteSettingEntity> existing, string key, string value)
    {
        var entity = existing.FirstOrDefault(x => x.Key == key);
        if (entity == null)
            _settingRepository.Insert(new SiteSettingEntity { Key = key, Value = value });
        else
            entity.Value = value;
    }

    /// <summary>
    /// Boş metin null değil "" olarak saklanır: anahtar kalır, varsayılan metin geri gelmez.
    /// </summary>
    private static string Clean(string value)
    {
        return (value ?? string.Empty).Replace("\r\n", "\n").Trim();
    }

    private List<StatItemModel> ReadStats(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new List<StatItemModel>();

        try
        {
            return JsonSerializer.Deserialize<List<StatItemModel>>(value) ?? new List<StatItemModel>();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Sayılarla Biz rakamları okunamadı.");
            return new List<StatItemModel>();
        }
    }
}
