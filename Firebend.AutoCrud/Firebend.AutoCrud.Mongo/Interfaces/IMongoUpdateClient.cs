using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Models.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoUpdateClient<TEntity, TKey>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<TEntity> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<TEntity> UpsertAsync(TEntity entity, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

        Task<List<TEntity>> UpsertManyAsync(List<EntityUpdate<TEntity>> entities, CancellationToken cancellationToken = default);

        Task<List<TOut>> UpsertManyAsync<TOut>(List<EntityUpdate<TEntity>> entities, Expression<Func<TEntity, TOut>> projection, CancellationToken cancellationToken = default);

        Task<TEntity> UpdateAsync(Guid id, JsonPatchDocument<TEntity> patch, CancellationToken cancellationToken = default);
    }
}