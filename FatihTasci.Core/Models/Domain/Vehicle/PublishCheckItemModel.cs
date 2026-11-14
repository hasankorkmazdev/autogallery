namespace FatihTasci.Core.Models.Domain.Vehicle;

public class PublishCheckItemModel
{
    public string Key { get; set; }
    public string Label { get; set; }
    public bool Passed { get; set; }

    /// <summary>
    /// true ise eksik olduğu sürece araç yayınlanamaz; false ise yalnızca uyarıdır.
    /// </summary>
    public bool Blocking { get; set; }

    /// <summary>
    /// Eksiği gidermek için gidilecek sihirbaz adımı (1-7)
    /// </summary>
    public int Step { get; set; }
}
