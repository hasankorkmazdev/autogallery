using FatihTasci.Core.Entity.Identity;
using Microsoft.EntityFrameworkCore;

namespace FatihTasci.Data.Context.Seed.Account;

/// <summary>
/// Panel kullanıcıları (AccountSeedData): Id'si de e-postası da veritabanında yoksa eklenir, varsa dokunulmaz (panelde
/// değişen ad, rol, şifre ve aktiflik kalır; silinmiş hesap geri gelmez). Geliştirmede test hesabı da eklenir.
/// </summary>
internal static class AccountSeeder
{
    public static async Task SeedAsync(AppDbContext context, bool isDevelopment)
    {
        var users = isDevelopment
            ? AccountSeedData.Users.Concat(AccountSeedData.DevelopmentUsers)
            : AccountSeedData.Users;
        var existing = await context.Users.IgnoreQueryFilters().AsNoTracking()
            .Select(x => new { x.Id, x.NormalizedEMail })
            .ToListAsync();

        foreach (var seed in users)
        {
            var email = seed.Email.ToLowerInvariant();
            if (existing.Any(x => x.Id == seed.Id || x.NormalizedEMail == email))
                continue;

            context.Users.Add(new UserEntity
            {
                Id = seed.Id,
                Name = seed.Name,
                Surname = seed.Surname,
                EMail = seed.Email,
                NormalizedEMail = email,
                PasswordHash = seed.PasswordHash,
                Role = seed.Role,
                IsActive = true
            });
        }

        await context.SaveChangesAsync();
    }
}
