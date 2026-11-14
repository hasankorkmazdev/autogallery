namespace FatihTasci.Core.Models.Domain.Setting;

/// <summary>
/// "Filigranı yeniden uygula" işinin durumu. Panel iş sürerken bu durumu düzenli aralıklarla sorar.
/// </summary>
public class WatermarkJobStatusModel
{
    public bool IsRunning { get; set; }
    public int Total { get; set; }
    public int Processed { get; set; }
    public int Failed { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
