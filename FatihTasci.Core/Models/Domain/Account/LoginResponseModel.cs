namespace FatihTasci.Core.Models.Domain.Account;

public class LoginResponseModel
{
    public UserModel User { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}
