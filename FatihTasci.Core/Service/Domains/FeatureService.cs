using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Enums;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Catalog;
using FatihTasci.Core.RepositoryBase;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.EntityFrameworkCore;

namespace FatihTasci.Core.Service.Domains;

/// <summary>
/// Donanım listesi. Sıra (<see cref="FeatureEntity.SortOrder"/>) kategori içindedir ve panelde sürükleyerek
/// değiştirilir; sihirbaz ve vitrin bu sırayı kullanır. Yeni donanım ya da kategorisi değişen donanım
/// kategorinin sonuna eklenir.
/// </summary>
public class FeatureService : DomainService, IFeatureService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<FeatureEntity> _featureRepository;

    public FeatureService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _featureRepository = unitOfWork.GetRepository<FeatureEntity>();
    }

    public async Task<ServiceResultModel<List<FeatureGroupModel>>> GetGroupedAsync(bool includeInactive)
    {
        return Result(await LoadGroupsAsync(includeInactive));
    }

    public async Task<ServiceResultModel<FeatureModel>> CreateAsync(SaveFeatureRequestModel request)
    {
        var name = request.Name.Trim();
        if (await _featureRepository.ExistsAsync(x => x.Category == request.Category && x.Name == name))
            return Result<FeatureModel>(null, "Bu kategoride aynı adda bir donanım zaten var.", 400);

        var feature = new FeatureEntity
        {
            Category = request.Category,
            Name = name,
            SortOrder = await NextSortOrderAsync(request.Category),
            IsActive = request.IsActive
        };
        _featureRepository.Insert(feature);
        await _unitOfWork.SaveChangesAsync();

        return Result(ToModel(feature), "Donanım eklendi.");
    }

    public async Task<ServiceResultModel<FeatureModel>> UpdateAsync(int id, SaveFeatureRequestModel request)
    {
        var feature = await _featureRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == id, disableTracking: false);
        if (feature == null)
            return Result<FeatureModel>(null, "Donanım bulunamadı.", 404);

        var name = request.Name.Trim();
        if (await _featureRepository.ExistsAsync(x => x.Category == request.Category && x.Name == name && x.Id != id))
            return Result<FeatureModel>(null, "Bu kategoride aynı adda bir donanım zaten var.", 400);

        if (feature.Category != request.Category)
            feature.SortOrder = await NextSortOrderAsync(request.Category);

        feature.Category = request.Category;
        feature.Name = name;
        feature.IsActive = request.IsActive;
        await _unitOfWork.SaveChangesAsync();

        return Result(ToModel(feature), "Donanım güncellendi.");
    }

    public async Task<ServiceResultModel<List<FeatureGroupModel>>> ReorderAsync(FeatureOrderRequestModel request)
    {
        var features = await _featureRepository.GetQuery(disableTracking: false)
            .Where(x => x.Category == request.Category)
            .ToListAsync();
        if (features.Count != request.FeatureIds.Count || features.Any(x => !request.FeatureIds.Contains(x.Id)))
            return Result<List<FeatureGroupModel>>(null, "Donanım listesi güncel değil, sayfayı yenileyin.", 400);

        foreach (var feature in features)
            feature.SortOrder = request.FeatureIds.IndexOf(feature.Id);

        await _unitOfWork.SaveChangesAsync();
        return Result(await LoadGroupsAsync(includeInactive: true), "Sıralama kaydedildi.");
    }

    private async Task<List<FeatureGroupModel>> LoadGroupsAsync(bool includeInactive)
    {
        var features = await _featureRepository.GetAllAsync(
            selector: x => new FeatureModel
            {
                Id = x.Id,
                Category = x.Category,
                Name = x.Name,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            },
            predicate: x => includeInactive || x.IsActive,
            orderBy: q => q.OrderBy(x => x.SortOrder).ThenBy(x => x.Name));

        return Enum.GetValues<FeatureCategory>()
            .Select(category => new FeatureGroupModel
            {
                Category = category,
                CategoryName = category.GetDisplayName(),
                Features = features.Where(x => x.Category == category).ToList()
            })
            .ToList();
    }

    private async Task<int> NextSortOrderAsync(FeatureCategory category)
    {
        var last = await _featureRepository.GetQuery()
            .Where(x => x.Category == category)
            .MaxAsync(x => (int?)x.SortOrder);
        return (last ?? -1) + 1;
    }

    private static FeatureModel ToModel(FeatureEntity x) => new()
    {
        Id = x.Id,
        Category = x.Category,
        Name = x.Name,
        SortOrder = x.SortOrder,
        IsActive = x.IsActive
    };
}
