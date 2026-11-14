namespace FatihTasci.Core.Models.Domain.Vehicle;

public class PublishCheckModel
{
    public bool CanPublish { get; set; }
    public List<PublishCheckItemModel> Items { get; set; } = new();
}
