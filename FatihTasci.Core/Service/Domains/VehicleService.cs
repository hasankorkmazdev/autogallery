using FatihTasci.Core.Entity.Catalog;
using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Enums;
using FatihTasci.Core.Extensions.Data;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Vehicle;
using FatihTasci.Core.Providers;
using FatihTasci.Core.RepositoryBase;
using FatihTasci.Core.Service.Domains.Abstracts;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;

namespace FatihTasci.Core.Service.Domains;

/// <summary>
/// Araç sihirbazının (7 adım) arka ucu. Her adım kendi uç noktasıyla taslağa kaydedilir.
/// </summary>
public class VehicleService : DomainService, IVehicleService
{
    public const int MinPhotoCountRecommended = 8;
    public const int MaxNewVehicleKilometer = 100;

    private const string NotFoundMessage = "Araç bulunamadı.";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<VehicleEntity> _vehicleRepository;
    private readonly IRepository<VehicleBodyPartEntity> _bodyPartRepository;
    private readonly IRepository<VehicleFeatureEntity> _vehicleFeatureRepository;
    private readonly IRepository<FeatureEntity> _featureRepository;
    private readonly IRepository<CarModelEntity> _modelRepository;
    private readonly IRepository<ColorEntity> _colorRepository;
    private readonly IMediaStorage _storage;

    public VehicleService(IUnitOfWork unitOfWork, IMediaStorage storage)
    {
        _unitOfWork = unitOfWork;
        _vehicleRepository = unitOfWork.GetRepository<VehicleEntity>();
        _bodyPartRepository = unitOfWork.GetRepository<VehicleBodyPartEntity>();
        _vehicleFeatureRepository = unitOfWork.GetRepository<VehicleFeatureEntity>();
        _featureRepository = unitOfWork.GetRepository<FeatureEntity>();
        _modelRepository = unitOfWork.GetRepository<CarModelEntity>();
        _colorRepository = unitOfWork.GetRepository<ColorEntity>();
        _storage = storage;
    }

    public static string BuildPublicUrl(string slug, int listingNo)
    {
        return string.IsNullOrEmpty(slug) ? $"/arac/{listingNo}" : $"/arac/{slug}-{listingNo}";
    }

    #region Liste ve detay

    public async Task<ServiceResultModel<IPagedList<VehicleListItemModel>>> GetListAsync(VehicleListRequestModel request)
    {
        var query = _vehicleRepository.GetQuery();

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);
        else
            query = query.Where(x => x.Status != VehicleStatus.Archived);

        if (request.BrandId.HasValue)
            query = query.Where(x => x.BrandId == request.BrandId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            var isNumber = int.TryParse(search, out var listingNo);
            query = query.Where(x =>
                x.Title.Contains(search) ||
                x.Brand.Name.Contains(search) ||
                x.Series.Name.Contains(search) ||
                x.CarModel.Name.Contains(search) ||
                (isNumber && x.ListingNo == listingNo));
        }

        var page = await query
            .OrderByDescending(x => x.LastUpdateDate ?? x.CreateDate)
            .Select(x => new
            {
                x.Id,
                x.ListingNo,
                x.Slug,
                x.Title,
                BrandName = x.Brand.Name,
                SeriesName = x.Series.Name,
                ModelName = x.CarModel.Name,
                x.Year,
                x.Kilometer,
                x.Price,
                x.ShowPrice,
                x.Status,
                x.IsFeatured,
                x.CreateDate,
                x.LastUpdateDate,
                CoverPath = x.Media.Where(m => m.IsCover).Select(m => m.SmallPath).FirstOrDefault(),
                PhotoCount = x.Media.Count()
            })
            .ToPagedListAsync(request.PageIndex, request.PageSize);

        var result = new PagedList<VehicleListItemModel>
        {
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
            TotalPages = page.TotalPages,
            IndexFrom = page.IndexFrom,
            Items = page.Items.Select(x => new VehicleListItemModel
            {
                Id = x.Id,
                ListingNo = x.ListingNo,
                Title = x.Title,
                BrandName = x.BrandName,
                SeriesName = x.SeriesName,
                ModelName = x.ModelName,
                Year = x.Year,
                Kilometer = x.Kilometer,
                Price = x.Price,
                ShowPrice = x.ShowPrice,
                Status = x.Status,
                IsFeatured = x.IsFeatured,
                CoverUrl = _storage.ToUrl(x.CoverPath),
                PhotoCount = x.PhotoCount,
                PublicUrl = BuildPublicUrl(x.Slug, x.ListingNo),
                CreateDate = x.CreateDate,
                LastUpdateDate = x.LastUpdateDate
            }).ToList()
        };

        return Result<IPagedList<VehicleListItemModel>>(result);
    }

    public async Task<ServiceResultModel<VehicleDetailModel>> GetDetailAsync(Guid id)
    {
        var detail = await LoadDetailAsync(id);
        return detail == null
            ? Result<VehicleDetailModel>(null, NotFoundMessage, 404)
            : Result(detail);
    }

    #endregion

    #region Adım 1: Kimlik

    public async Task<ServiceResultModel<VehicleDetailModel>> CreateAsync(VehicleIdentityRequestModel request)
    {
        var catalog = await LoadCatalogChainAsync(request);
        if (catalog == null)
            return Result<VehicleDetailModel>(null, "Seçilen marka, seri ve model birbiriyle uyumlu değil.", 400);

        var vehicle = new VehicleEntity
        {
            Id = Guid.NewGuid(),
            BrandId = request.BrandId,
            SeriesId = request.SeriesId,
            CarModelId = request.CarModelId,
            Title = request.Title.Trim(),
            Status = VehicleStatus.Draft,
            Slug = SlugHelper.ForVehicle(null, catalog.Series.Brand.Name, catalog.Name)
        };

        _vehicleRepository.Insert(vehicle);
        await _unitOfWork.SaveChangesAsync();

        return Result(await LoadDetailAsync(vehicle.Id), "Taslak oluşturuldu.");
    }

    public async Task<ServiceResultModel<VehicleDetailModel>> UpdateIdentityAsync(Guid id, VehicleIdentityRequestModel request)
    {
        var vehicle = await GetTrackedAsync(id);
        if (vehicle == null)
            return Result<VehicleDetailModel>(null, NotFoundMessage, 404);

        var catalog = await LoadCatalogChainAsync(request);
        if (catalog == null)
            return Result<VehicleDetailModel>(null, "Seçilen marka, seri ve model birbiriyle uyumlu değil.", 400);

        vehicle.BrandId = request.BrandId;
        vehicle.SeriesId = request.SeriesId;
        vehicle.CarModelId = request.CarModelId;
        vehicle.Title = request.Title.Trim();
        vehicle.Slug = SlugHelper.ForVehicle(vehicle.Year, catalog.Series.Brand.Name, catalog.Name);

        return await SaveAndReturnAsync(id, "Kimlik bilgileri kaydedildi.");
    }

    #endregion

    #region Adım 2-3: Teknik ve durum

    public async Task<ServiceResultModel<VehicleDetailModel>> UpdateTechnicalAsync(Guid id, VehicleTechnicalRequestModel request)
    {
        var vehicle = await GetTrackedAsync(id, includeCatalog: true);
        if (vehicle == null)
            return Result<VehicleDetailModel>(null, NotFoundMessage, 404);

        if (vehicle.Condition == VehicleCondition.New && request.Kilometer > MaxNewVehicleKilometer)
            return Result<VehicleDetailModel>(null, "Sıfır araçta kilometre 0 ile 100 arasında olmalıdır.", 400);

        if (request.ExteriorColorId.HasValue && !await _colorRepository.ExistsAsync(x => x.Id == request.ExteriorColorId))
            return Result<VehicleDetailModel>(null, "Seçilen renk bulunamadı.", 400);

        vehicle.Year = request.Year;
        vehicle.Kilometer = request.Kilometer;
        vehicle.FuelType = request.FuelType;
        vehicle.Transmission = request.Transmission;
        vehicle.BodyType = request.BodyType;
        vehicle.EngineCc = request.EngineCc;
        vehicle.PowerHp = request.PowerHp;
        vehicle.Drivetrain = request.Drivetrain;
        vehicle.BatteryKwh = request.BatteryKwh;
        vehicle.RangeKm = request.RangeKm;
        vehicle.ExteriorColorId = request.ExteriorColorId;
        vehicle.InteriorColor = request.InteriorColor?.Trim();
        vehicle.Slug = SlugHelper.ForVehicle(vehicle.Year, vehicle.Brand.Name, vehicle.CarModel.Name);

        return await SaveAndReturnAsync(id, "Teknik bilgiler kaydedildi.");
    }

    public async Task<ServiceResultModel<VehicleDetailModel>> UpdateConditionAsync(Guid id, VehicleConditionRequestModel request)
    {
        var vehicle = await GetTrackedAsync(id);
        if (vehicle == null)
            return Result<VehicleDetailModel>(null, NotFoundMessage, 404);

        if (request.Condition == VehicleCondition.New && vehicle.Kilometer > MaxNewVehicleKilometer)
            return Result<VehicleDetailModel>(null, "Sıfır araçta kilometre 0 ile 100 arasında olmalıdır. Önce Teknik adımında kilometreyi düzeltin.", 400);

        vehicle.Condition = request.Condition;
        vehicle.HasWarranty = request.HasWarranty;
        vehicle.WarrantyEndDate = request.HasWarranty ? request.WarrantyEndDate?.Date : null;
        vehicle.HeavyDamageRecord = request.HeavyDamageRecord;
        vehicle.PlateOrigin = request.PlateOrigin;
        vehicle.AcceptsTrade = request.AcceptsTrade;
        vehicle.TramerAmount = request.TramerAmount;

        return await SaveAndReturnAsync(id, "Durum bilgileri kaydedildi.");
    }

    #endregion

    #region Adım 4-5: Boya / değişen ve donanım

    public async Task<ServiceResultModel<VehicleDetailModel>> UpdateBodyPartsAsync(Guid id, VehicleBodyPartsRequestModel request)
    {
        if (!await _vehicleRepository.ExistsAsync(x => x.Id == id))
            return Result<VehicleDetailModel>(null, NotFoundMessage, 404);

        var existing = await _bodyPartRepository.GetQuery(disableTracking: false)
            .Where(x => x.VehicleId == id)
            .ToListAsync();
        _bodyPartRepository.Delete(existing);

        // 13 parçanın hepsi yazılır; gönderilmeyen parça Orijinal kabul edilir.
        var parts = Enum.GetValues<BodyPart>().Select(part =>
        {
            var item = request.Parts.FirstOrDefault(x => x.Part == part);
            return new VehicleBodyPartEntity
            {
                VehicleId = id,
                Part = part,
                State = item?.State ?? BodyPartState.Original,
                Note = string.IsNullOrWhiteSpace(item?.Note) ? null : item.Note.Trim()
            };
        });
        _bodyPartRepository.Insert(parts);

        return await SaveAndReturnAsync(id, "Boya / değişen bilgileri kaydedildi.");
    }

    public async Task<ServiceResultModel<VehicleDetailModel>> UpdateFeaturesAsync(Guid id, VehicleFeaturesRequestModel request)
    {
        if (!await _vehicleRepository.ExistsAsync(x => x.Id == id))
            return Result<VehicleDetailModel>(null, NotFoundMessage, 404);

        var requestedIds = request.FeatureIds.Distinct().ToList();
        var validIds = await _featureRepository.GetAllAsync(selector: x => x.Id, predicate: x => requestedIds.Contains(x.Id));
        if (validIds.Count != requestedIds.Count)
            return Result<VehicleDetailModel>(null, "Seçilen donanımlardan bazıları bulunamadı.", 400);

        var existing = await _vehicleFeatureRepository.GetQuery(disableTracking: false)
            .Where(x => x.VehicleId == id)
            .ToListAsync();
        _vehicleFeatureRepository.Delete(existing);
        _vehicleFeatureRepository.Insert(validIds.Select(featureId => new VehicleFeatureEntity { VehicleId = id, FeatureId = featureId }));

        return await SaveAndReturnAsync(id, "Donanım bilgileri kaydedildi.");
    }

    /// <summary>
    /// "Aynı modelin önceki ilanındaki donanımı getir" kısayolu.
    /// </summary>
    public async Task<ServiceResultModel<List<int>>> GetFeatureSuggestionAsync(Guid id)
    {
        var modelId = await _vehicleRepository.GetFirstOrDefaultAsync(selector: x => (int?)x.CarModelId, predicate: x => x.Id == id);
        if (modelId == null)
            return Result<List<int>>(null, NotFoundMessage, 404);

        var sourceId = await _vehicleRepository.GetFirstOrDefaultAsync(
            selector: x => (Guid?)x.Id,
            predicate: x => x.CarModelId == modelId && x.Id != id && x.Features.Any(),
            orderBy: q => q.OrderByDescending(x => x.CreateDate));

        if (sourceId == null)
            return Result(new List<int>(), "Bu modelin donanımı girilmiş başka bir ilanı yok.", 404);

        var featureIds = await _vehicleFeatureRepository.GetAllAsync(selector: x => x.FeatureId, predicate: x => x.VehicleId == sourceId);
        return Result(featureIds.ToList(), "Önceki ilanın donanımı getirildi.");
    }

    #endregion

    #region Adım 6-7: Video, fiyat ve yayın

    public async Task<ServiceResultModel<VehicleDetailModel>> UpdateVideoAsync(Guid id, VehicleVideoRequestModel request)
    {
        var vehicle = await GetTrackedAsync(id);
        if (vehicle == null)
            return Result<VehicleDetailModel>(null, NotFoundMessage, 404);

        vehicle.VideoUrl = string.IsNullOrWhiteSpace(request.VideoUrl) ? null : request.VideoUrl.Trim();
        return await SaveAndReturnAsync(id, "Video bağlantısı kaydedildi.");
    }

    public async Task<ServiceResultModel<VehicleDetailModel>> UpdatePricingAsync(Guid id, VehiclePricingRequestModel request)
    {
        var vehicle = await GetTrackedAsync(id);
        if (vehicle == null)
            return Result<VehicleDetailModel>(null, NotFoundMessage, 404);

        if (vehicle.Status == VehicleStatus.Published && !(request.Price > 0))
            return Result<VehicleDetailModel>(null, "Yayındaki bir aracın fiyatı boş ya da 0 olamaz.", 400);

        vehicle.Price = request.Price;
        vehicle.ShowPrice = request.ShowPrice;
        vehicle.Description = SanitizeDescription(request.Description);
        vehicle.IsFeatured = request.IsFeatured;
        vehicle.FeaturedOrder = request.FeaturedOrder;

        return await SaveAndReturnAsync(id, "Fiyat ve açıklama kaydedildi.");
    }

    public async Task<ServiceResultModel<PublishCheckModel>> GetPublishCheckAsync(Guid id)
    {
        var vehicle = await _vehicleRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == id);
        if (vehicle == null)
            return Result<PublishCheckModel>(null, NotFoundMessage, 404);

        return Result(await BuildPublishCheckAsync(vehicle));
    }

    public async Task<ServiceResultModel<VehicleDetailModel>> UpdateStatusAsync(Guid id, VehicleStatusRequestModel request)
    {
        var vehicle = await GetTrackedAsync(id);
        if (vehicle == null)
            return Result<VehicleDetailModel>(null, NotFoundMessage, 404);

        if (request.Status is VehicleStatus.Published or VehicleStatus.Reserved)
        {
            var check = await BuildPublishCheckAsync(vehicle);
            if (!check.CanPublish)
            {
                var missing = string.Join(", ", check.Items.Where(x => x.Blocking && !x.Passed).Select(x => x.Label));
                return Result<VehicleDetailModel>(null, $"Araç yayınlanamaz. Eksikler: {missing}", 400);
            }
            vehicle.PublishedAt ??= DateTime.Now;
        }

        vehicle.SoldAt = request.Status == VehicleStatus.Sold ? vehicle.SoldAt ?? DateTime.Now : null;
        vehicle.Status = request.Status;

        return await SaveAndReturnAsync(id, $"Durum \"{request.Status.GetDisplayName()}\" olarak değiştirildi.");
    }

    /// <summary>
    /// Benzer ilandan kopyala: katalog, teknik bilgiler (km ve renkler hariç), durum ve donanım kopyalanır.
    /// Hasar, fotoğraf ve fiyat yeni araç için ayrıca girilir.
    /// </summary>
    public async Task<ServiceResultModel<VehicleDetailModel>> CopyAsync(Guid id)
    {
        var source = await _vehicleRepository.GetFirstOrDefaultAsync(
            predicate: x => x.Id == id,
            include: q => q.Include(x => x.Features));
        if (source == null)
            return Result<VehicleDetailModel>(null, NotFoundMessage, 404);

        var copy = new VehicleEntity
        {
            Id = Guid.NewGuid(),
            Status = VehicleStatus.Draft,
            Slug = source.Slug,
            BrandId = source.BrandId,
            SeriesId = source.SeriesId,
            CarModelId = source.CarModelId,
            Title = source.Title,
            Year = source.Year,
            FuelType = source.FuelType,
            Transmission = source.Transmission,
            BodyType = source.BodyType,
            EngineCc = source.EngineCc,
            PowerHp = source.PowerHp,
            Drivetrain = source.Drivetrain,
            BatteryKwh = source.BatteryKwh,
            RangeKm = source.RangeKm,
            Condition = source.Condition,
            PlateOrigin = source.PlateOrigin,
            AcceptsTrade = source.AcceptsTrade,
            ShowPrice = source.ShowPrice
        };
        _vehicleRepository.Insert(copy);
        _vehicleFeatureRepository.Insert(source.Features.Select(x => new VehicleFeatureEntity { VehicleId = copy.Id, FeatureId = x.FeatureId }));
        await _unitOfWork.SaveChangesAsync();

        return Result(await LoadDetailAsync(copy.Id), "İlan kopyalandı. Kilometre, renk, hasar, fotoğraf ve fiyatı girin.");
    }

    #endregion

    #region Yardımcılar

    private async Task<PublishCheckModel> BuildPublishCheckAsync(VehicleEntity vehicle)
    {
        var photoCount = await _unitOfWork.GetRepository<VehicleMediaEntity>().CountAsync(x => x.VehicleId == vehicle.Id);
        var hasCover = await _unitOfWork.GetRepository<VehicleMediaEntity>().ExistsAsync(x => x.VehicleId == vehicle.Id && x.IsCover);
        var isElectric = vehicle.FuelType == FuelType.Electric;
        var kmValid = vehicle.Kilometer.HasValue &&
                      (vehicle.Condition != VehicleCondition.New || vehicle.Kilometer <= MaxNewVehicleKilometer);

        var items = new List<PublishCheckItemModel>
        {
            Item("title", "İlan başlığı", !string.IsNullOrWhiteSpace(vehicle.Title), true, 1),
            Item("year", "Model yılı", vehicle.Year.HasValue, true, 2),
            Item("kilometer", "Kilometre (sıfır araçta en fazla 100)", kmValid, true, 2),
            Item("fuel", "Yakıt, vites ve kasa tipi", vehicle.FuelType.HasValue && vehicle.Transmission.HasValue && vehicle.BodyType.HasValue, true, 2),
            Item("engine", "Motor hacmi ve gücü", vehicle.PowerHp.HasValue && (isElectric || vehicle.EngineCc.HasValue), true, 2),
            Item("color", "Dış renk", vehicle.ExteriorColorId.HasValue, true, 2),
            Item("photos", "En az bir fotoğraf", photoCount > 0, true, 6),
            Item("cover", "Kapak fotoğrafı seçildi", hasCover, true, 6),
            Item("photos-recommended", $"En az {MinPhotoCountRecommended} fotoğraf (şu an {photoCount})", photoCount >= MinPhotoCountRecommended, false, 6),
            Item("price", "Fiyat (0'dan büyük)", vehicle.Price > 0, true, 7)
        };

        return new PublishCheckModel
        {
            Items = items,
            CanPublish = items.Where(x => x.Blocking).All(x => x.Passed)
        };
    }

    private static PublishCheckItemModel Item(string key, string label, bool passed, bool blocking, int step) => new()
    {
        Key = key,
        Label = label,
        Passed = passed,
        Blocking = blocking,
        Step = step
    };

    private static string SanitizeDescription(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear();
        foreach (var tag in new[] { "p", "br", "strong", "b", "em", "i", "u", "ul", "ol", "li", "h3", "h4" })
            sanitizer.AllowedTags.Add(tag);
        sanitizer.AllowedAttributes.Clear();

        return sanitizer.Sanitize(html.Trim());
    }

    private async Task<CarModelEntity> LoadCatalogChainAsync(VehicleIdentityRequestModel request)
    {
        return await _modelRepository.GetFirstOrDefaultAsync(
            predicate: x => x.Id == request.CarModelId &&
                            x.SeriesId == request.SeriesId &&
                            x.Series.BrandId == request.BrandId,
            include: q => q.Include(x => x.Series).ThenInclude(x => x.Brand));
    }

    private async Task<VehicleEntity> GetTrackedAsync(Guid id, bool includeCatalog = false)
    {
        return await _vehicleRepository.GetFirstOrDefaultAsync(
            predicate: x => x.Id == id,
            include: includeCatalog ? q => q.Include(x => x.Brand).Include(x => x.CarModel) : null,
            disableTracking: false);
    }

    private async Task<ServiceResultModel<VehicleDetailModel>> SaveAndReturnAsync(Guid id, string message)
    {
        await _unitOfWork.SaveChangesAsync();
        return Result(await LoadDetailAsync(id), message);
    }

    private async Task<VehicleDetailModel> LoadDetailAsync(Guid id)
    {
        var x = await _vehicleRepository.GetQuery()
            .Include(v => v.Brand)
            .Include(v => v.Series)
            .Include(v => v.CarModel)
            .Include(v => v.BodyParts)
            .Include(v => v.Features)
            .Include(v => v.Media)
            .AsSplitQuery()
            .FirstOrDefaultAsync(v => v.Id == id);

        if (x == null)
            return null;

        return new VehicleDetailModel
        {
            Id = x.Id,
            ListingNo = x.ListingNo,
            Slug = x.Slug,
            Status = x.Status,
            PublicUrl = BuildPublicUrl(x.Slug, x.ListingNo),
            BrandId = x.BrandId,
            SeriesId = x.SeriesId,
            CarModelId = x.CarModelId,
            BrandName = x.Brand.Name,
            SeriesName = x.Series.Name,
            ModelName = x.CarModel.Name,
            Title = x.Title,
            Year = x.Year,
            Kilometer = x.Kilometer,
            FuelType = x.FuelType,
            Transmission = x.Transmission,
            BodyType = x.BodyType,
            EngineCc = x.EngineCc,
            PowerHp = x.PowerHp,
            Drivetrain = x.Drivetrain,
            BatteryKwh = x.BatteryKwh,
            RangeKm = x.RangeKm,
            ExteriorColorId = x.ExteriorColorId,
            InteriorColor = x.InteriorColor,
            Condition = x.Condition,
            HasWarranty = x.HasWarranty,
            WarrantyEndDate = x.WarrantyEndDate,
            HeavyDamageRecord = x.HeavyDamageRecord,
            PlateOrigin = x.PlateOrigin,
            AcceptsTrade = x.AcceptsTrade,
            TramerAmount = x.TramerAmount,
            BodyParts = Enum.GetValues<BodyPart>().Select(part =>
            {
                var saved = x.BodyParts.FirstOrDefault(p => p.Part == part);
                return new VehicleBodyPartItemModel
                {
                    Part = part,
                    State = saved?.State ?? BodyPartState.Original,
                    Note = saved?.Note
                };
            }).ToList(),
            FeatureIds = x.Features.Select(f => f.FeatureId).ToList(),
            Media = x.Media.OrderBy(m => m.SortOrder).Select(m => MediaService.ToModel(m, _storage)).ToList(),
            VideoUrl = x.VideoUrl,
            ExpertiseReportUrl = _storage.ToUrl(x.ExpertiseReportPath),
            Price = x.Price,
            ShowPrice = x.ShowPrice,
            Description = x.Description,
            IsFeatured = x.IsFeatured,
            FeaturedOrder = x.FeaturedOrder,
            ViewCount = x.ViewCount,
            PublishedAt = x.PublishedAt,
            SoldAt = x.SoldAt
        };
    }

    #endregion
}
