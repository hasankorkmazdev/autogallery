using FatihTasci.Core.Entity.Base;
using FatihTasci.Core.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FatihTasci.Core.RepositoryBase;

public interface IUnitOfWork : IDisposable
{
    DbContext Context { get; }

    IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

    /// <summary>
    /// Denetim alanlarını doldurur, silinen ISoftDeletableEntity kayıtlarını işaretleyip kaydeder.
    /// </summary>
    Task<int> SaveChangesAsync();

    Task<IDbContextTransaction> BeginTransactionAsync();
}

public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    TContext DbContext { get; }
}

/// <summary>
/// Signage'daki UnitOfWork'ün tenant'sız hali. ConfigureEntries yalnızca denetim alanlarını
/// ve soft delete'i yönetir.
/// </summary>
public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
{
    private readonly TContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly Dictionary<Type, object> _repositories = new();
    private bool _disposed;

    public UnitOfWork(TContext context, ICurrentUserProvider currentUserProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUserProvider = currentUserProvider;
    }

    public TContext DbContext => _context;

    public DbContext Context => _context;

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);
        if (!_repositories.TryGetValue(type, out var repository))
        {
            repository = new Repository<TEntity>(_context);
            _repositories[type] = repository;
        }

        return (IRepository<TEntity>)repository;
    }

    public async Task<int> SaveChangesAsync()
    {
        ConfigureEntries();
        return await _context.SaveChangesAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    private void ConfigureEntries()
    {
        var userId = _currentUserProvider?.GetUserId();
        var now = DateTime.Now;

        foreach (var item in _context.ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (item.State)
            {
                case EntityState.Modified:
                    item.Entity.LastUpdateDate = now;
                    item.Entity.LastUpdatedBy = userId;
                    break;
                case EntityState.Added:
                    item.Entity.CreateDate = now;
                    item.Entity.CreatedBy ??= userId;
                    break;
            }
        }

        foreach (var item in _context.ChangeTracker.Entries<ISoftDeletableEntity>())
        {
            if (item.State != EntityState.Deleted)
                continue;

            item.State = EntityState.Modified;
            item.Entity.IsDelete = true;
            item.Entity.LastDeletedBy = userId;
            item.Entity.LastDeleteDate = now;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        // DbContext'in ömrünü DI yönetir; burada yalnızca repository önbelleği temizlenir.
        if (!_disposed && disposing)
            _repositories.Clear();

        _disposed = true;
    }
}
