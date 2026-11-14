using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Base;

public abstract class BaseEntity<T> : IEntity<T>
{
    /// <summary>
    /// Clustered Primary Key
    /// Override edilerek Customize edilebilir.
    /// </summary>
    [Key]
    [Column(Order = 0)]
    public virtual T Id { get; set; }

    /// <summary>
    /// Aktif mi ?
    /// </summary>
    [Column(Order = 1)]
    public bool IsActive { get; set; } = true;
}
