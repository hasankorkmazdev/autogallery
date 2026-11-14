using FatihTasci.Core.Enums;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Account;
using FatihTasci.Core.Models.Domain.Catalog;
using FatihTasci.Core.Models.Domain.Lead;
using FatihTasci.Core.Models.Domain.Public;
using FatihTasci.Core.Models.Domain.Setting;
using FatihTasci.Core.Models.Domain.Showroom;
using FatihTasci.Core.Models.Domain.Vehicle;
using Microsoft.AspNetCore.Http;

namespace FatihTasci.Core.Service.Domains.Abstracts;

public interface IAccountService : IDomainService
{
    Task<ServiceResultModel<LoginResponseModel>> LoginAsync(LoginRequestModel request);
    Task<ServiceResultModel<UserModel>> GetUserAsync(Guid userId);

    /// <summary>
    /// Token'daki kullanıcı hâlâ aktif ve rolü aynı mı? (Pasife alınan ya da rolü değişen kullanıcının eski token'ı geçmez.)
    /// </summary>
    Task<bool> IsSessionValidAsync(Guid userId, string role);
}

public interface IUserService : IDomainService
{
    Task<ServiceResultModel<List<UserListItemModel>>> GetListAsync();
    Task<ServiceResultModel<UserListItemModel>> CreateAsync(SaveUserRequestModel request);
    Task<ServiceResultModel<UserListItemModel>> UpdateAsync(Guid id, SaveUserRequestModel request);
    Task<ServiceResultModel> ResetPasswordAsync(Guid id, ResetUserPasswordRequestModel request);
}

public interface ICatalogService : IDomainService
{
    Task<ServiceResultModel<List<BrandModel>>> GetBrandsAsync(bool includeInactive);
    Task<ServiceResultModel<BrandModel>> CreateBrandAsync(SaveBrandRequestModel request);
    Task<ServiceResultModel<BrandModel>> UpdateBrandAsync(int id, SaveBrandRequestModel request);

    Task<ServiceResultModel<List<SeriesModel>>> GetSeriesAsync(int brandId, bool includeInactive);
    Task<ServiceResultModel<SeriesModel>> CreateSeriesAsync(SaveSeriesRequestModel request);
    Task<ServiceResultModel<SeriesModel>> UpdateSeriesAsync(int id, SaveSeriesRequestModel request);

    Task<ServiceResultModel<List<CarModelModel>>> GetModelsAsync(int seriesId, bool includeInactive);
    Task<ServiceResultModel<CarModelModel>> CreateModelAsync(SaveCarModelRequestModel request);
    Task<ServiceResultModel<CarModelModel>> UpdateModelAsync(int id, SaveCarModelRequestModel request);

    Task<ServiceResultModel<List<ColorModel>>> GetColorsAsync();
}

public interface IFeatureService : IDomainService
{
    Task<ServiceResultModel<List<FeatureGroupModel>>> GetGroupedAsync(bool includeInactive);
    Task<ServiceResultModel<FeatureModel>> CreateAsync(SaveFeatureRequestModel request);
    Task<ServiceResultModel<FeatureModel>> UpdateAsync(int id, SaveFeatureRequestModel request);
}

public interface IVehicleService : IDomainService
{
    Task<ServiceResultModel<IPagedList<VehicleListItemModel>>> GetListAsync(VehicleListRequestModel request);
    Task<ServiceResultModel<VehicleDetailModel>> GetDetailAsync(Guid id);
    Task<ServiceResultModel<VehicleDetailModel>> CreateAsync(VehicleIdentityRequestModel request);
    Task<ServiceResultModel<VehicleDetailModel>> UpdateIdentityAsync(Guid id, VehicleIdentityRequestModel request);
    Task<ServiceResultModel<VehicleDetailModel>> UpdateTechnicalAsync(Guid id, VehicleTechnicalRequestModel request);
    Task<ServiceResultModel<VehicleDetailModel>> UpdateConditionAsync(Guid id, VehicleConditionRequestModel request);
    Task<ServiceResultModel<VehicleDetailModel>> UpdateBodyPartsAsync(Guid id, VehicleBodyPartsRequestModel request);
    Task<ServiceResultModel<VehicleDetailModel>> UpdateFeaturesAsync(Guid id, VehicleFeaturesRequestModel request);
    Task<ServiceResultModel<List<int>>> GetFeatureSuggestionAsync(Guid id);
    Task<ServiceResultModel<VehicleDetailModel>> UpdateVideoAsync(Guid id, VehicleVideoRequestModel request);
    Task<ServiceResultModel<VehicleDetailModel>> UpdatePricingAsync(Guid id, VehiclePricingRequestModel request);
    Task<ServiceResultModel<PublishCheckModel>> GetPublishCheckAsync(Guid id);
    Task<ServiceResultModel<VehicleDetailModel>> UpdateStatusAsync(Guid id, VehicleStatusRequestModel request);
    Task<ServiceResultModel<VehicleDetailModel>> CopyAsync(Guid id);
}

public interface IMediaService : IDomainService
{
    Task<ServiceResultModel<List<VehicleMediaModel>>> UploadVehicleImagesAsync(Guid vehicleId, IFormFileCollection files);
    Task<ServiceResultModel<List<VehicleMediaModel>>> ReorderAsync(Guid vehicleId, VehicleMediaOrderRequestModel request);
    Task<ServiceResultModel<List<VehicleMediaModel>>> SetCoverAsync(Guid vehicleId, Guid mediaId);
    Task<ServiceResultModel<List<VehicleMediaModel>>> DeleteAsync(Guid vehicleId, Guid mediaId);
    Task<ServiceResultModel<string>> UploadExpertiseReportAsync(Guid vehicleId, IFormFile file);
    Task<ServiceResultModel> DeleteExpertiseReportAsync(Guid vehicleId);

    /// <summary>
    /// Tüm fotoğrafların WebP varyantlarını güncel filigran ayarıyla orijinallerden yeniden üretir.
    /// onStart toplam sayıyı, onItem her fotoğrafın sonucunu (başarılı mı) bildirir.
    /// </summary>
    Task RegenerateAllVariantsAsync(Action<int> onStart, Action<bool> onItem, CancellationToken cancellationToken);
}

public interface IShowroomService : IDomainService
{
    Task<ServiceResultModel<List<ShowroomSceneModel>>> GetAllAsync();

    /// <summary>
    /// Vitrin: aktif ve fotoğrafı olan sahneler, sırasıyla
    /// </summary>
    Task<List<ShowroomSceneModel>> GetPublicAsync();

    Task<ServiceResultModel<List<ShowroomSceneModel>>> CreateAsync(SaveShowroomSceneRequestModel request);
    Task<ServiceResultModel<List<ShowroomSceneModel>>> UpdateAsync(int id, SaveShowroomSceneRequestModel request);
    Task<ServiceResultModel<List<ShowroomSceneModel>>> ReorderAsync(ShowroomSceneOrderRequestModel request);
    Task<ServiceResultModel<List<ShowroomSceneModel>>> DeleteAsync(int id);
    Task<ServiceResultModel<List<ShowroomSceneModel>>> UploadImageAsync(int id, IFormFile file);
    Task<ServiceResultModel<List<ShowroomSceneModel>>> DeleteImageAsync(int id);
}

public interface ILeadService : IDomainService
{
    Task<ServiceResultModel> CreateAsync(CreateLeadRequestModel request, string ipAddress);
    Task<ServiceResultModel<IPagedList<LeadModel>>> GetListAsync(LeadListRequestModel request);
    Task<ServiceResultModel<LeadModel>> UpdateAsync(Guid id, UpdateLeadRequestModel request);
    Task<ServiceResultModel<int>> GetNewCountAsync();
}

public interface ISiteSettingService : IDomainService
{
    Task<SiteSettingsModel> GetAsync();
    Task<ServiceResultModel<SiteSettingsModel>> UpdateAsync(SiteSettingsModel request);
    Task<ServiceResultModel<SiteSettingsModel>> UploadLogoAsync(IFormFile file);
    Task<ServiceResultModel<SiteSettingsModel>> DeleteLogoAsync();
}

/// <summary>
/// Tüm araç fotoğraflarının varyantlarını orijinallerden arka planda yeniden üretir (logo / filigran değişince).
/// Tek örnek (singleton) çalışır; aynı anda yalnız bir iş yürür.
/// </summary>
public interface IMediaRegenerationRunner
{
    WatermarkJobStatusModel GetStatus();

    /// <summary>
    /// İş zaten sürüyorsa false döner.
    /// </summary>
    bool TryStart();
}

/// <summary>
/// Vitrin sayfalarının sorguları. Yalnızca yayındaki / rezerve / satılmış araçları görür.
/// </summary>
public interface IPublicVehicleService : IDomainService
{
    Task<HomePageModel> GetHomeAsync();
    Task<CatalogPathResult> ResolveCatalogPathAsync(string brandSlug, string seriesSlug);
    Task<StockPageModel> GetStockAsync(StockFilterModel filter, CatalogPathResult path);
    Task<PublicVehicleDetailModel> GetVehicleAsync(int listingNo);
    Task<Dictionary<string, List<EnumOption>>> GetFilterOptionsAsync();
}

public class CatalogPathResult
{
    public bool Found { get; set; }

    /// <summary>
    /// Eski slug ile gelindiyse güncel adres (301 ile yönlendirilir)
    /// </summary>
    public string RedirectPath { get; set; }

    public int? BrandId { get; set; }
    public string BrandName { get; set; }
    public string BrandSlug { get; set; }
    public int? SeriesId { get; set; }
    public string SeriesName { get; set; }
    public string SeriesSlug { get; set; }
}
