using System.Linq.Expressions;

public interface IPagedRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    Task<PagedResult<TEntity>> GetPagedAsync(
       Expression<Func<TEntity, bool>>? filter = null,
       string? sortBy = null,
       bool sortDesc = false,
       int page = 1,
       int pageSize = 20,
       CancellationToken ct = default,
       params Expression<Func<TEntity, object>>[] includes
   );


}
