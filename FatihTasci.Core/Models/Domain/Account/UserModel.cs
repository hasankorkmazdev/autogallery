using FatihTasci.Core.Enums;

namespace FatihTasci.Core.Models.Domain.Account;

public class UserModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string EMail { get; set; }
    public UserRole Role { get; set; }
}
