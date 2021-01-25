using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoReadClient<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken);

        Task<IMongoQueryable<TEntity>> GetQueryableAsync(FilterDefinition<TEntity> firstPipelineStateFilters, CancellationToken cancellationToken = default);

        Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

        Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

        Task<EntityPagedResponse<TEntity>> GetPagedResponseAsync<TSearchRequest>(IMongoQueryable<TEntity> queryable,
            TSearchRequest searchRequest,
            CancellationToken cancellationToken = default)
            where TSearchRequest : EntitySearchRequest;
    }
}
