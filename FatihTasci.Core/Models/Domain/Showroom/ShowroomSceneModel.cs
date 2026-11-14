namespace FatihTasci.Core.Models.Domain.Showroom;

public class ShowroomSceneModel
{
    public int Id { get; set; }
    public string Tag { get; set; }
    public string Title { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }

    /// <summary>
    /// 1280 px WebP (panel önizlemesi, mobil kart)
    /// </summary>
    public string MediumUrl { get; set; }

    /// <summary>
    /// 2400 px WebP (masaüstü geniş panel, tam ekran)
    /// </summary>
    public string LargeUrl { get; set; }
}
