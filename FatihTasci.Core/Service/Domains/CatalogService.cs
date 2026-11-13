using FatihTasci.Core.Entity.Catalog;
using FatihTasci.Core.Entity.Common;
using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Enums;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Catalog;
using FatihTasci.Core.RepositoryBase;
using FatihTasci.Core.Service.Domains.Abstracts;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace FatihTasci.Core.Service.Domains;

/// <summary>
/// Marka → Seri → Model kataloğu ve renkler. Slug'lar addan otomatik üretilir;
/// ad değişince eski slug SlugRedirect'e yazılır (PROJE_DOKUMANI §3.8).
/// </summary>
public class CatalogService : DomainService, ICatalogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<BrandEntity> _brandRepository;
    private readonly IRepository<SeriesEntity> _seriesRepository;
    private readonly IRepository<CarModelEntity> _modelRepository;
    private readonly IRepository<ColorEntity> _colorRepository;
    private readonly IRepository<SlugRedirectEntity> _redirectRepository;
    private readonly IRepository<VehicleEntity> _vehicleRepository;

    public CatalogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _brandRepository = unitOfWork.GetRepository<BrandEntity>();
        _seriesRepository = unitOfWork.GetRepository<SeriesEntity>();
        _modelRepository = unitOfWork.GetRepository<CarModelEntity>();
        _colorRepository = unitOfWork.GetRepository<ColorEntity>();
        _redirectRepository = unitOfWork.GetRepository<SlugRedirectEntity>();
        _vehicleRepository = unitOfWork.GetRepository<VehicleEntity>();
    }

    #region Brand

    public async Task<ServiceResultModel<List<BrandModel>>> GetBrandsAsync(bool includeInactive)
    {
        var brands = await _brandRepository.GetAllAsync(
            selector: x => new BrandModel
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                SeriesCount = x.Series.Count(s => !s.IsDelete)
            },
            predicate: x => includeInactive || x.IsActive,
            orderBy: q => q.OrderBy(x => x.SortOrder).ThenBy(x => x.Name));

        return Result(brands.ToList());
    }

    public async Task<ServiceResultModel<BrandModel>> CreateBrandAsync(SaveBrandRequestModel request)
    {
        var name = request.Name.Trim();
        if (await _brandRepository.ExistsAsync(x => x.Name == name))
            return Result<BrandModel>(null, "Bu adda bir marka zaten var.", 400);

        var brand = new BrandEntity
        {
            Name = name,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            Slug = await UniqueBrandSlugAsync(name, null)
        };
        _brandRepository.Insert(brand);
        await _unitOfWork.SaveChangesAsync();

        return Result(ToModel(brand), "Marka eklendi.");
    }

    public async Task<ServiceResultModel<BrandModel>> UpdateBrandAsync(int id, SaveBrandRequestModel request)
    {
        var brand = await _brandRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == id, disableTracking: false);
        if (brand == null)
            return Result<BrandModel>(null, "Marka bulunamadı.", 404);

        var name = request.Name.Trim();
        if (await _brandRepository.ExistsAsync(x => x.Name == name && x.Id != id))
            return Result<BrandModel>(null, "Bu adda bir marka zaten var.", 400);

        var nameChanged = brand.Name != name;
        brand.Name = name;
        brand.SortOrder = request.SortOrder;
        brand.IsActive = request.IsActive;

        if (nameChanged)
        {
            var newSlug = await UniqueBrandSlugAsync(name, id);
            await ChangeSlugAsync(SlugEntityType.Brand, null, id, brand.Slug, newSlug);
            brand.Slug = newSlug;
            await RefreshVehicleSlugsAsync(x => x.BrandId == id, brandNameOverride: name);
        }

        await _unitOfWork.SaveChangesAsync();
        return Result(ToModel(brand), "Marka güncellendi.");
    }

    #endregion

    #region Series

    public async Task<ServiceResultModel<List<SeriesModel>>> GetSeriesAsync(int brandId, bool includeInactive)
    {
        var series = await _seriesRepository.GetAllAsync(
            selector: x => new SeriesModel
            {
                Id = x.Id,
                BrandId = x.BrandId,
                Name = x.Name,
                Slug = x.Slug,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                ModelCount = x.Models.Count(m => !m.IsDelete)
            },
            predicate: x => x.BrandId == brandId && (includeInactive || x.IsActive),
            orderBy: q => q.OrderBy(x => x.SortOrder).ThenBy(x => x.Name));

        return Result(series.ToList());
    }

    public async Task<ServiceResultModel<SeriesModel>> CreateSeriesAsync(SaveSeriesRequestModel request)
    {
        if (!await _brandRepository.ExistsAsync(x => x.Id == request.BrandId))
            return Result<SeriesModel>(null, "Marka bulunamadı.", 404);

        var name = request.Name.Trim();
        if (await _seriesRepository.ExistsAsync(x => x.BrandId == request.BrandId && x.Name == name))
            return Result<SeriesModel>(null, "Bu markada aynı adda bir seri zaten var.", 400);

        var series = new SeriesEntity
        {
            BrandId = request.BrandId,
            Name = name,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            Slug = await UniqueSeriesSlugAsync(request.BrandId, name, null)
        };
        _seriesRepository.Insert(series);
        await _unitOfWork.SaveChangesAsync();

        return Result(ToModel(series), "Seri eklendi.");
    }

    public async Task<ServiceResultModel<SeriesModel>> UpdateSeriesAsync(int id, SaveSeriesRequestModel request)
    {
        var series = await _seriesRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == id, disableTracking: false);
        if (series == null)
            return Result<SeriesModel>(null, "Seri bulunamadı.", 404);

        if (series.BrandId != request.BrandId)
            return Result<SeriesModel>(null, "Serinin markası değiştirilemez.", 400);

        var name = request.Name.Trim();
        if (await _seriesRepository.ExistsAsync(x => x.BrandId == series.BrandId && x.Name == name && x.Id != id))
            return Result<SeriesModel>(null, "Bu markada aynı adda bir seri zaten var.", 400);

        var nameChanged = series.Name != name;
        series.Name = name;
        series.SortOrder = request.SortOrder;
        series.IsActive = request.IsActive;

        if (nameChanged)
        {
            var newSlug = await UniqueSeriesSlugAsync(series.BrandId, name, id);
            await ChangeSlugAsync(SlugEntityType.Series, series.BrandId, id, series.Slug, newSlug);
            series.Slug = newSlug;
        }

        await _unitOfWork.SaveChangesAsync();
        return Result(ToModel(series), "Seri güncellendi.");
    }

    #endregion

    #region Model

    public async Task<ServiceResultModel<List<CarModelModel>>> GetModelsAsync(int seriesId, bool includeInactive)
    {
        var models = await _modelRepository.GetAllAsync(
            selector: x => new CarModelModel
            {
                Id = x.Id,
                SeriesId = x.SeriesId,
                Name = x.Name,
                Slug = x.Slug,
                IsActive = x.IsActive
            },
            predicate: x => x.SeriesId == seriesId && (includeInactive || x.IsActive),
            orderBy: q => q.OrderBy(x => x.Name));

        return Result(models.ToList());
    }

    public async Task<ServiceResultModel<CarModelModel>> CreateModelAsync(SaveCarModelRequestModel request)
    {
        if (!await _seriesRepository.ExistsAsync(x => x.Id == request.SeriesId))
            return Result<CarModelModel>(null, "Seri bulunamadı.", 404);

        var name = request.Name.Trim();
        if (await _modelRepository.ExistsAsync(x => x.SeriesId == request.SeriesId && x.Name == name))
            return Result<CarModelModel>(null, "Bu seride aynı adda bir model zaten var.", 400);

        var model = new CarModelEntity
        {
            SeriesId = request.SeriesId,
            Name = name,
            IsActive = request.IsActive,
            Slug = await UniqueModelSlugAsync(request.SeriesId, name, null)
        };
        _modelRepository.Insert(model);
        await _unitOfWork.SaveChangesAsync();

        return Result(ToModel(model), "Model eklendi.");
    }

    public async Task<ServiceResultModel<CarModelModel>> UpdateModelAsync(int id, SaveCarModelRequestModel request)
    {
        var model = await _modelRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == id, disableTracking: false);
        if (model == null)
            return Result<CarModelModel>(null, "Model bulunamadı.", 404);

        if (model.SeriesId != request.SeriesId)
            return Result<CarModelModel>(null, "Modelin serisi değiştirilemez.", 400);

        var name = request.Name.Trim();
        if (await _modelRepository.ExistsAsync(x => x.SeriesId == model.SeriesId && x.Name == name && x.Id != id))
            return Result<CarModelModel>(null, "Bu seride aynı adda bir model zaten var.", 400);

        var nameChanged = model.Name != name;
        model.Name = name;
        model.IsActive = request.IsActive;

        if (nameChanged)
        {
            var newSlug = await UniqueModelSlugAsync(model.SeriesId, name, id);
            await ChangeSlugAsync(SlugEntityType.CarModel, model.SeriesId, id, model.Slug, newSlug);
            model.Slug = newSlug;
            await RefreshVehicleSlugsAsync(x => x.CarModelId == id, modelNameOverride: name);
        }

        await _unitOfWork.SaveChangesAsync();
        return Result(ToModel(model), "Model güncellendi.");
    }

    #endregion

    public async Task<ServiceResultModel<List<ColorModel>>> GetColorsAsync()
    {
        var colors = await _colorRepository.GetAllAsync(
            selector: x => new ColorModel { Id = x.Id, Name = x.Name, HexCode = x.HexCode },
            predicate: x => x.IsActive,
            orderBy: q => q.OrderBy(x => x.SortOrder).ThenBy(x => x.Name));

        return Result(colors.ToList());
    }

    #region Slug

    private Task<string> UniqueBrandSlugAsync(string name, int? exceptId)
    {
        return SlugHelper.EnsureUniqueAsync(SlugHelper.Generate(name),
            slug => _brandRepository.ExistsAsync(x => x.Slug == slug && x.Id != exceptId));
    }

    private Task<string> UniqueSeriesSlugAsync(int brandId, string name, int? exceptId)
    {
        return SlugHelper.EnsureUniqueAsync(SlugHelper.Generate(name),
            slug => _seriesRepository.ExistsAsync(x => x.BrandId == brandId && x.Slug == slug && x.Id != exceptId));
    }

    private Task<string> UniqueModelSlugAsync(int seriesId, string name, int? exceptId)
    {
        return SlugHelper.EnsureUniqueAsync(SlugHelper.Generate(name),
            slug => _modelRepository.ExistsAsync(x => x.SeriesId == seriesId && x.Slug == slug && x.Id != exceptId));
    }

    /// <summary>
    /// Eski slug'ı yönlendirme tablosuna yazar. Yeni slug daha önce yönlendirme olarak kayıtlıysa
    /// döngü oluşmasın diye o kayıt silinir.
    /// </summary>
    private async Task ChangeSlugAsync(SlugEntityType type, int? parentId, int entityId, string oldSlug, string newSlug)
    {
        if (oldSlug == newSlug)
            return;

        var stale = await _redirectRepository.GetQuery(disableTracking: false)
            .Where(x => x.EntityType == type && x.ParentId == parentId && (x.OldSlug == newSlug || x.OldSlug == oldSlug))
            .ToListAsync();
        _redirectRepository.Delete(stale);

        _redirectRepository.Insert(new SlugRedirectEntity
        {
            EntityType = type,
            ParentId = parentId,
            OldSlug = oldSlug,
            EntityId = entityId
        });
    }

    /// <summary>
    /// Marka ya da model adı değişince o araçların slug'ı yeniden üretilir.
    /// Araç sayfası ilan no ile bulunduğu için eski adresler 301 ile yeni adrese gider.
    /// </summary>
    private async Task RefreshVehicleSlugsAsync(
        Expression<Func<VehicleEntity, bool>> predicate,
        string brandNameOverride = null,
        string modelNameOverride = null)
    {
        var vehicles = await _vehicleRepository.GetQuery(disableTracking: false)
            .Include(x => x.Brand)
            .Include(x => x.CarModel)
            .Where(predicate)
            .ToListAsync();

        foreach (var vehicle in vehicles)
        {
            vehicle.Slug = SlugHelper.ForVehicle(
                vehicle.Year,
                brandNameOverride ?? vehicle.Brand.Name,
                modelNameOverride ?? vehicle.CarModel.Name);
        }
    }

    #endregion

    private static BrandModel ToModel(BrandEntity x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Slug = x.Slug,
        SortOrder = x.SortOrder,
        IsActive = x.IsActive
    };

    private static SeriesModel ToModel(SeriesEntity x) => new()
    {
        Id = x.Id,
        BrandId = x.BrandId,
        Name = x.Name,
        Slug = x.Slug,
        SortOrder = x.SortOrder,
        IsActive = x.IsActive
    };

    private static CarModelModel ToModel(CarModelEntity x) => new()
    {
        Id = x.Id,
        SeriesId = x.SeriesId,
        Name = x.Name,
        Slug = x.Slug,
        IsActive = x.IsActive
    };
}
