using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Base;

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
