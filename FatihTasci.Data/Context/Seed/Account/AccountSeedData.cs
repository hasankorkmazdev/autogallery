using FatihTasci.Core.Enums;

namespace FatihTasci.Data.Context.Seed.Account;

/// <summary>
/// Panel kullanıcıları. Şifre koda düz yazılmaz: PasswordHash, PasswordHasher&lt;UserEntity&gt; (PBKDF2) özetidir.
/// Şifre değişecekse yeni özet üretilip buraya yazılır.
/// </summary>
internal static class AccountSeedData
{
    public static readonly (Guid Id, string Name, string Surname, string Email, string PasswordHash, UserRole Role)[] Users =
    [
        (new Guid("3f1c9a52-7d4e-4b8a-9c21-6e5f0d8b7a13"), "Galeri", "Yöneticisi", "fatih@fatihtasci.com.tr",
            "AQAAAAIAAYagAAAAEFtSayHhDVspB3tWC2YG05iQbdjY059sYQ+nMjLzGjzRrUgVqBUzfJdeJy0ZkK+mZA==", UserRole.Admin),
        // Seed'deki ilanların satış danışmanı (ListingSeedData.AdvisorId). Şifresi bilinmeyen rastgele bir şifrenin
        // özetidir: panele girmesi için Yönetici Kullanıcılar sayfasından şifre belirler.
        (new Guid("fb30d4ee-060c-44b0-b2a4-00b8b7278a0e"), "Hasan", "Korkmaz", "hasan@fatihtasci.com.tr",
            "AQAAAAIAAYagAAAAENj6y0WVHsD7V4+aBllCRK9GFRjF+VmTEB/WUYGSRn4Js+9sOJdAqUsUnKGtPLvRtg==", UserRole.Staff)
    ];

    /// <summary>
    /// Yalnız geliştirme ortamında eklenir: panelin geliştirme girişi bu hesapla hazır dolu gelir (login-form.tsx).
    /// </summary>
    public static readonly (Guid Id, string Name, string Surname, string Email, string PasswordHash, UserRole Role)[] DevelopmentUsers =
    [
        (new Guid("a7e2d4c1-5b3f-4e9a-8d6c-2f1b0e9a4c57"), "Test", "Yönetici", "admin@galeri.test",
            "AQAAAAIAAYagAAAAEP05NyVoNdY7BRUPnwxG4n/Ykx1xvM/trXmkZa0NxBbdTNZW50hRwHTmneBit1TDMA==", UserRole.Admin)
    ];
}
