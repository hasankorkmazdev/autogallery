using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Base;

public interface IEntity<T>
{
    public T Id { get; set; }
}

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

public interface IAuditableEntity
{
    public DateTime CreateDate { get; set; }
    public DateTime? LastUpdateDate { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? LastUpdatedBy { get; set; }
}

public interface ISoftDeletableEntity
{
    public bool IsDelete { get; set; }
    public DateTime? LastDeleteDate { get; set; }
    public Guid? LastDeletedBy { get; set; }
}

/// <summary>
/// Denetim alanları (oluşturma / güncelleme / silme) ve soft delete taşıyan temel entity.
/// Alanlar UnitOfWork.SaveChangesAsync içinde otomatik doldurulur.
/// </summary>
public class BaseAuditableEntity<T> : BaseEntity<T>, ISoftDeletableEntity, IAuditableEntity
{
    /// <summary>
    /// Silindi mi ?
    /// </summary>
    [Column(Order = 2)]
    public bool IsDelete { get; set; } = false;

    /// <summary>
    /// Oluşturma Tarihi
    /// </summary>
    [Column(Order = 1000)]
    public DateTime CreateDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Son Güncelleme Tarihi
    /// </summary>
    [Column(Order = 1001)]
    public DateTime? LastUpdateDate { get; set; }

    /// <summary>
    /// Son Silinme Tarihi
    /// </summary>
    [Column(Order = 1002)]
    public DateTime? LastDeleteDate { get; set; }

    /// <summary>
    /// Kaydı Oluşturan Kullanıcı
    /// </summary>
    [Column(Order = 1003)]
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Kaydı Son Silen Kullanıcı
    /// </summary>
    [Column(Order = 1004)]
    public Guid? LastDeletedBy { get; set; }

    /// <summary>
    /// Kaydı Son Güncelleyen Kullanıcı
    /// </summary>
    [Column(Order = 1005)]
    public Guid? LastUpdatedBy { get; set; }
}
