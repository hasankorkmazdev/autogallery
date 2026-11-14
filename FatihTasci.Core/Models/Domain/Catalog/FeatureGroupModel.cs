using FatihTasci.Core.Enums;

namespace FatihTasci.Core.Models.Domain.Catalog;

public class FeatureGroupModel
{
    public FeatureCategory Category { get; set; }
    public string CategoryName { get; set; }
    public List<FeatureModel> Features { get; set; } = new();
}
