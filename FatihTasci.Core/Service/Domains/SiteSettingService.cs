using FatihTasci.Core.Entity.Common;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Setting;
using FatihTasci.Core.RepositoryBase;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace FatihTasci.Core.Service.Domains;

/// <summary>
/// SiteSettingsModel'in her özelliği SiteSettings tablosunda aynı adlı bir anahtardır.
/// Vitrin her istekte okuduğu için sonuç bellekte tutulur, güncellemede temizlenir.
/// </summary>
public class SiteSettingService : DomainService, ISiteSettingService
{
    private const string CacheKey = "site-settings";
    private static readonly PropertyInfo[] Properties = typeof(SiteSettingsModel).GetProperties();

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<SiteSettingEntity> _settingRepository;
    private readonly IMemoryCache _cache;

    public SiteSettingService(IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _settingRepository = unitOfWork.GetRepository<SiteSettingEntity>();
        _cache = cache;
    }

    public async Task<SiteSettingsModel> GetAsync()
    {
        if (_cache.TryGetValue(CacheKey, out SiteSettingsModel cached))
            return cached;

        var values = await _settingRepository.GetQuery().ToDictionaryAsync(x => x.Key, x => x.Value);
        var model = new SiteSettingsModel();
        foreach (var property in Properties)
        {
            if (values.TryGetValue(property.Name, out var value))
                property.SetValue(model, value);
        }

        _cache.Set(CacheKey, model, TimeSpan.FromMinutes(10));
        return model;
    }

    public async Task<ServiceResultModel<SiteSettingsModel>> UpdateAsync(SiteSettingsModel request)
    {
        var existing = await _settingRepository.GetQuery(disableTracking: false).ToListAsync();

        foreach (var property in Properties)
        {
            var value = (property.GetValue(request) as string)?.Trim();
            if (string.IsNullOrEmpty(value))
                value = null;

            var entity = existing.FirstOrDefault(x => x.Key == property.Name);
            if (entity == null)
                _settingRepository.Insert(new SiteSettingEntity { Key = property.Name, Value = value });
            else
                entity.Value = value;
        }

        await _unitOfWork.SaveChangesAsync();
        _cache.Remove(CacheKey);

        return Result(await GetAsync(), "Ayarlar kaydedildi.");
    }
}
