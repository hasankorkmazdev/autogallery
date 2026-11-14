using FatihTasci.Core.Enums;

namespace FatihTasci.Core.Models.Domain.Account;

/// <summary>
/// Kullanıcı yönetimi ekranındaki satır
/// </summary>
public class UserListItemModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string EMail { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime CreateDate { get; set; }
}
