using System;
using System.Linq;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.CustomCommands;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Searching;

public class EntityFrameworkChangeTrackingSearchHandler<TKey, TEntity> : IEntitySearchHandler<Guid, ChangeTrackingEntity<TKey, TEntity>, ChangeTrackingSearchRequest<TKey>>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    public IQueryable<ChangeTrackingEntity<TKey, TEntity>> HandleSearch(IQueryable<ChangeTrackingEntity<TKey, TEntity>> query, ChangeTrackingSearchRequest<TKey> searchRequest)
    {
        if (string.IsNullOrWhiteSpace(searchRequest.Search))
        {
            return query;
        }

        if (!searchRequest.Search.Contains('%'))
        {
            searchRequest.Search = $"%{searchRequest.Search}%";
        }

        query = query.Where(x =>
            EF.Functions.JsonContainsAny(x.Changes, searchRequest.Search) ||
            EF.Functions.JsonContainsAny(x.Entity, searchRequest.Search));

        return query;
    }
}
