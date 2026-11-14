namespace FatihTasci.Core.Entity.Base;

public interface IAuditableEntity
{
    public DateTime CreateDate { get; set; }
    public DateTime? LastUpdateDate { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? LastUpdatedBy { get; set; }
}
