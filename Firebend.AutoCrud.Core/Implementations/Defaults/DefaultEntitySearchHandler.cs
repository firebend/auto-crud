using System;
using System.Linq;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.Core.Implementations.Defaults;

public class DefaultEntitySearchHandler<TKey, TEntity, TSearch> : IEntitySearchHandler<TKey, TEntity, TSearch>
    where TKey : struct
    where TEntity : IEntity<TKey>
    where TSearch : IEntitySearchRequest
{
    private readonly Func<IQueryable<TEntity>, TSearch, IQueryable<TEntity>> _func;
    private readonly Func<IQueryable<TEntity>, TSearch, Task<IQueryable<TEntity>>> _asyncFunc;

    public DefaultEntitySearchHandler(Func<IQueryable<TEntity>, TSearch, IQueryable<TEntity>> func,
        Func<IQueryable<TEntity>, TSearch, Task<IQueryable<TEntity>>> asyncFunc = null)
    {
        _func = func;
        _asyncFunc = asyncFunc;
    }

    public IQueryable<TEntity> HandleSearch(IQueryable<TEntity> queryable, TSearch searchRequest) =>
        _func == null ? queryable : _func(queryable, searchRequest);

    public Task<IQueryable<TEntity>> HandleSearchAsync(IQueryable<TEntity> queryable, TSearch searchRequest) =>
        _asyncFunc == null ? Task.FromResult(queryable) : _asyncFunc(queryable, searchRequest);
}
