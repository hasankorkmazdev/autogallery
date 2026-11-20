using System.Globalization;
using FatihTasci.Core.Entity.Base;
using Microsoft.EntityFrameworkCore;

namespace FatihTasci.Data.Context.Seed;

/// <summary>
/// Seed yalnız ekler: koddaki kayıt veritabanında yoksa aynı Id ile eklenir, varsa dokunulmaz (panelde değiştirilmiş
/// olsa da). Seed hiçbir kaydı güncellemez ve silmez.
/// </summary>
internal static class SeedHelper
{
    /// <summary>
    /// Ad / slug karşılaştırması: Türkçe, büyük-küçük harf farkı yok.
    /// </summary>
    public static readonly StringComparer NameComparer = StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), true);

    /// <summary>
    /// int Id'li (identity) tablo için: Id'si de <paramref name="key"/> ile verilen benzersiz alanı (ad, slug) da
    /// veritabanında bulunmayan seed kayıtlarını ekler. Aynı adlı kayıt panelden eklenmişse seed'deki atlanır, kopya
    /// oluşmaz. Silinmiş (IsDelete) kayıtlar da var sayılır, geri getirilmez. Seed Id'leri 100000'in altındadır; panelden
    /// eklenen kayıtlar 100000'den başlar (AppDbContext.SeedIdLimit), böylece seed'e sonradan eklenen kayıt panelin
    /// kaydıyla çakışmaz.
    /// </summary>
    /// <param name="prepare">Eklenecek her kayıt için eklemeden önce çalışır (sıra, fotoğraf).</param>
    public static async Task AddMissingAsync<T>(AppDbContext context, IEnumerable<T> seeds, Func<T, string> key,
        Func<T, Task> prepare = null) where T : BaseEntity<int>
    {
        var existing = await context.Set<T>().IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var ids = existing.Select(x => x.Id).ToHashSet();
        var keys = existing.Select(x => key(x) ?? string.Empty).ToHashSet(NameComparer);

        foreach (var seed in seeds)
        {
            var seedKey = key(seed) ?? string.Empty;
            if (ids.Contains(seed.Id) || keys.Contains(seedKey))
                continue;

            if (prepare != null)
                await prepare(seed);
            context.Set<T>().Add(seed);
            ids.Add(seed.Id);
            keys.Add(seedKey);
        }

        await SaveWithIdsAsync<T>(context);
    }

    /// <summary>
    /// Kaydeder; Id'si verilmiş yeni kayıtlar için T'nin tablosunda IDENTITY_INSERT açılır.
    /// </summary>
    public static async Task SaveWithIdsAsync<T>(AppDbContext context) where T : class
    {
        var hasNew = context.ChangeTracker.Entries<T>().Any(x => x.State == EntityState.Added);
        if (!hasNew)
        {
            await context.SaveChangesAsync();
            return;
        }

        var entityType = context.Model.FindEntityType(typeof(T));
        // Tablo adı modelden gelir (kullanıcı girdisi değil); SET IDENTITY_INSERT parametre almaz.
        var table = $"[{entityType.GetSchema()}].[{entityType.GetTableName()}]";
        var identityOn = $"SET IDENTITY_INSERT {table} ON";
        var identityOff = $"SET IDENTITY_INSERT {table} OFF";

        await using var transaction = await context.Database.BeginTransactionAsync();
        await context.Database.ExecuteSqlRawAsync(identityOn);
        await context.SaveChangesAsync();
        await context.Database.ExecuteSqlRawAsync(identityOff);
        await transaction.CommitAsync();
    }
}
