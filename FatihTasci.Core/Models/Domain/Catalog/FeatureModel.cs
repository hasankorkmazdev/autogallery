using FatihTasci.Core.Enums;

namespace FatihTasci.Core.Models.Domain.Catalog;

public class FeatureModel
{
    public int Id { get; set; }
    public FeatureCategory Category { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
