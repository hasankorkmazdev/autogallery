using FatihTasci.Core.Entity.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Crm;

/// <summary>
/// "Aracını Sat" formundan yüklenen fotoğraflar.
/// </summary>
[Table(name: "LeadMedia", Schema = "Crm")]
public class LeadMediaEntity : BaseEntity<Guid>
{
    public Guid LeadId { get; set; }
    public string Path { get; set; }

    public virtual LeadEntity Lead { get; set; }
}
