using System.Text.Json.Serialization;

namespace FatihTasci.Core.Models.Domain.Account;

public class LoginResponseModel
{
    public UserModel User { get; set; }

    /// <summary>
    /// Yanıt gövdesinde dönmez; AuthController HttpOnly çereze yazar (JS okuyamaz).
    /// </summary>
    [JsonIgnore]
    public string Token { get; set; }

    public DateTime ExpiresAt { get; set; }
}
