namespace FatihTasci.Core.Models.Domain.Common;

/// <summary>
/// Panelden yüklenen video (araç ya da teslimat): MP4 olduğu gibi /media altında, afiş videonun ilk karesi (WebP).
/// Width / Height afişin boyutudur, oranı videoyla aynıdır (dikey video için).
/// </summary>
public class VideoFileModel
{
    public string Url { get; set; }
    public string PosterUrl { get; set; }

    /// <summary>
    /// Küçük afiş (araç galerisinin küçük resmi); teslimatta yoktur.
    /// </summary>
    public string PosterSmallUrl { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }
}
