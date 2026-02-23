using System;
using System.Linq.Expressions;
using System.Threading;
using Application.DTOs;

public interface IRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> Query();
    Task<TEntity?> GetByFileNumberAsync(object id, CancellationToken ct = default);

    Task<TEntity?> GetByIdAsync(object id, CancellationToken ct = default, params Expression<Func<TEntity, object>>[] includes);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    Task UpdateAsync(TEntity entity, CancellationToken ct = default);
    Task DeleteAsync(TEntity entity, CancellationToken ct = default);
    Task<TEntity?> GetByFileNumberAsync(string fileNumber, CancellationToken ct = default);

}
