using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Enums;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Catalog;
using FatihTasci.Core.RepositoryBase;
using FatihTasci.Core.Service.Domains.Abstracts;

namespace FatihTasci.Core.Service.Domains;

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

        var groups = Enum.GetValues<FeatureCategory>()
            .Select(category => new FeatureGroupModel
            {
                Category = category,
                CategoryName = category.GetDisplayName(),
                Features = features.Where(x => x.Category == category).ToList()
            })
            .ToList();

        return Result(groups);
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
            SortOrder = request.SortOrder,
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

        feature.Category = request.Category;
        feature.Name = name;
        feature.SortOrder = request.SortOrder;
        feature.IsActive = request.IsActive;
        await _unitOfWork.SaveChangesAsync();

        return Result(ToModel(feature), "Donanım güncellendi.");
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
