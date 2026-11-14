using FatihTasci.Core.Enums;

namespace FatihTasci.Core.Models.Domain.Public;

// Vitrin (Razor) sayfalarının modelleri. Değerler ekranda gösterilecek şekilde hazırlanır.

public class BodyPartViewModel
{
    public BodyPart Part { get; set; }
    public string PartName { get; set; }
    public BodyPartState State { get; set; }
    public string StateName { get; set; }
    public string Note { get; set; }
}
