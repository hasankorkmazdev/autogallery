namespace FatihTasci.Core.Entity.Base;

public interface ISoftDeletableEntity
{
    public bool IsDelete { get; set; }
    public DateTime? LastDeleteDate { get; set; }
    public Guid? LastDeletedBy { get; set; }
}
