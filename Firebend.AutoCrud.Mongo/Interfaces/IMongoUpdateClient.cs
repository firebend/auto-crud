using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoUpdateClient<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    public Task<TEntity> UpdateAsync(TEntity entity,
        CancellationToken cancellationToken);

    public Task<TEntity> UpsertAsync(TEntity entity,
        CancellationToken cancellationToken);

    public Task<TEntity> UpsertAsync(TEntity entity,
        IEntityTransaction transaction,
        CancellationToken cancellationToken);

    public Task<TEntity> UpsertAsync(TEntity entity,
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken);

    public Task<TEntity> UpsertAsync(TEntity entity,
        Expression<Func<TEntity, bool>> filter,
        IEntityTransaction transaction,
        CancellationToken cancellationToken);

    public Task<List<TEntity>> UpsertManyAsync(List<EntityUpdate<TEntity>> entities,
        CancellationToken cancellationToken);

    public Task<List<TEntity>> UpsertManyAsync(List<EntityUpdate<TEntity>> entities,
        IEntityTransaction transaction,
        CancellationToken cancellationToken);

    public Task<List<TOut>> UpsertManyAsync<TOut>(List<EntityUpdate<TEntity>> entities,
        Expression<Func<TEntity, TOut>> projection,
        CancellationToken cancellationToken);

    public Task<List<TOut>> UpsertManyAsync<TOut>(List<EntityUpdate<TEntity>> entities,
        Expression<Func<TEntity, TOut>> projection,
        IEntityTransaction transaction,
        CancellationToken cancellationToken);

    public Task<TEntity> UpdateAsync(TKey id,
        JsonPatchDocument<TEntity> patch,
        CancellationToken cancellationToken);

    public Task<TEntity> UpdateAsync(TKey id,
        JsonPatchDocument<TEntity> patch,
        IEntityTransaction transaction,
        CancellationToken cancellationToken);
}
