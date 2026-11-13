using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace FatihTasci.Core.RepositoryBase;

/// <summary>
/// Signage'daki IRepository'nin tenant'sız, sadeleştirilmiş hali. Metot adları ve isimli parametreler
/// (predicate, include, orderBy, selector, disableTracking) Signage ile aynıdır.
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> GetQuery(bool disableTracking = true, bool ignoreQueryFilters = false);

    Task<TEntity> FindAsync(params object[] keyValues);

    Task<TEntity> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false);

    Task<TResult> GetFirstOrDefaultAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        bool ignoreQueryFilters = false);

    Task<IList<TResult>> GetAllAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        bool ignoreQueryFilters = false);

    Task<IPagedList<TResult>> GetPagedListAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, bool ignoreQueryFilters = false);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null, bool ignoreQueryFilters = false);

    void Insert(TEntity entity);
    void Insert(IEnumerable<TEntity> entities);
    void Update(TEntity entity);

    /// <summary>
    /// ISoftDeletableEntity ise UnitOfWork kaydı silinmiş olarak işaretler, değilse kalıcı siler.
    /// </summary>
    void Delete(TEntity entity);
    void Delete(IEnumerable<TEntity> entities);
}
