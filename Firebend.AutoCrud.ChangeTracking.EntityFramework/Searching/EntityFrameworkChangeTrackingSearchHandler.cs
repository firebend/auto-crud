using System;
using System.Linq;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.CustomCommands;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Searching;

// A thin closure of the generic 3-arg version over the default row type, kept as its own named
// class since it predates custom row types and is part of the public API.
public class EntityFrameworkChangeTrackingSearchHandler<TKey, TEntity>
    : EntityFrameworkChangeTrackingSearchHandler<TKey, TEntity, ChangeTrackingEntity<TKey, TEntity>>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
}

/// <summary>
/// Handles full text search for a custom <typeparamref name="TChangeTrackingEntity"/> change tracking row type.
/// </summary>
public class EntityFrameworkChangeTrackingSearchHandler<TKey, TEntity, TChangeTrackingEntity>
    : IEntitySearchHandler<Guid, TChangeTrackingEntity, ChangeTrackingSearchRequest<TKey>>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
    where TChangeTrackingEntity : ChangeTrackingEntity<TKey, TEntity>
{
    public IQueryable<TChangeTrackingEntity> HandleSearch(IQueryable<TChangeTrackingEntity> query, ChangeTrackingSearchRequest<TKey> searchRequest)
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
