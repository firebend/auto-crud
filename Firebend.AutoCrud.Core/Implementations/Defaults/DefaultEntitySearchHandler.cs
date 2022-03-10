using System;
using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.Core.Implementations.Defaults;

public class DefaultEntitySearchHandler<TKey, TEntity, TSearch> : IEntitySearchHandler<TKey, TEntity, TSearch>
    where TKey : struct
    where TEntity : IEntity<TKey>
    where TSearch : IEntitySearchRequest
{
    private readonly Func<IQueryable<TEntity>, TSearch, IQueryable<TEntity>> _func;

    public DefaultEntitySearchHandler(Func<IQueryable<TEntity>, TSearch, IQueryable<TEntity>> func)
    {
        _func = func;
    }

    public IQueryable<TEntity> HandleSearch(IQueryable<TEntity> queryable, TSearch searchRequest) =>
        _func == null ? queryable : _func(queryable, searchRequest);
}
