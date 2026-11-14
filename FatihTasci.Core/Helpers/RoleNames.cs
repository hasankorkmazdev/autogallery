using FatihTasci.Core.Enums;

namespace FatihTasci.Core.Helpers;

/// <summary>
/// JWT'deki rol claim'inin değerleri (UserRole enum adı). [Authorize(Roles = RoleNames.Admin)] ile kullanılır.
/// Yetkiler: CLAUDE.md "Kararlar" bölümü.
/// </summary>
public static class RoleNames
{
    public const string Admin = nameof(UserRole.Admin);
    public const string Staff = nameof(UserRole.Staff);
}
