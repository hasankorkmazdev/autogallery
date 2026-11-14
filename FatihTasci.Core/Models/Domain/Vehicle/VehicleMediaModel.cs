namespace FatihTasci.Core.Models.Domain.Vehicle;

public class VehicleMediaModel
{
    public Guid Id { get; set; }
    public string SmallUrl { get; set; }
    public string MediumUrl { get; set; }
    public string LargeUrl { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int SortOrder { get; set; }
    public bool IsCover { get; set; }
}
