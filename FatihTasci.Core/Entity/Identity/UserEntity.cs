using FatihTasci.Core.Entity.Base;
using FatihTasci.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Identity;

/// <summary>
/// Panel kullanıcısı. Kayıt ekranı yoktur; kullanıcılar seed ile ya da yönetici tarafından eklenir.
/// Şifre ASP.NET Core PasswordHasher (PBKDF2) ile saklanır.
/// </summary>
[Table(name: "Users", Schema = "Identity")]
public class UserEntity : BaseAuditableEntity<Guid>
{
    [Column(Order = 100)]
    public string Name { get; set; }

    [Column(Order = 101)]
    public string Surname { get; set; }

    [Column(Order = 102)]
    public string EMail { get; set; }

    /// <summary>
    /// Büyük/küçük harf duyarsız arama için e-postanın küçük harfli hali
    /// </summary>
    [Column(Order = 103)]
    public string NormalizedEMail { get; set; }

    [Column(Order = 104)]
    public string PasswordHash { get; set; }

    [Column(Order = 105)]
    public UserRole Role { get; set; } = UserRole.Admin;

    [Column(Order = 106)]
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Sitede danışmanı olduğu ilanlarda görünen iletişim telefonu (kişinin kendi numarası, galeri telefonundan farklı
    /// olabilir). Boşsa ilanda galeri telefonu kullanılır; cep telefonuysa WhatsApp da bu numaraya gider.
    /// </summary>
    [Column(Order = 107)]
    public string Phone { get; set; }
}
