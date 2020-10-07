using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DnsClient.Internal;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.AutoCrud.Core.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.Mongo.Abstractions
{
    public abstract class MongoClientBaseEntity<TEntity, TKey> : MongoClientBase
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        private readonly IMongoEntityConfiguration _entityConfiguration;
        
        protected MongoClientBaseEntity(IMongoClient client,
            ILogger logger,
            IMongoEntityConfiguration entityConfiguration) : base(client, logger)
        {
            _entityConfiguration = entityConfiguration;
        }
        
        protected IMongoCollection<TEntity> GetCollection()
        {
            var database = Client.GetDatabase(_entityConfiguration.DatabaseName);

            return database.GetCollection<TEntity>(_entityConfiguration.CollectionName);
        }
        
        protected IMongoQueryable<TEntity> GetFilteredCollection(FilterDefinition<TEntity> firstStageFilters = null)
        {
            var mongoQueryable = GetCollection().AsQueryable();

            if (firstStageFilters != null)
            {
                mongoQueryable = mongoQueryable.Where(_ => firstStageFilters.Inject());
            }

            var securityFilters = BuildFilters();

            return securityFilters == null ? mongoQueryable : mongoQueryable.Where(BuildFilters());
        }
        
        protected Expression<Func<TEntity, bool>> BuildFilters(Expression<Func<TEntity, bool>> additionalFilter = null)
        {
            var securityFilters = GetSecurityFilters() ?? new List<Expression<Func<TEntity, bool>>>();

            var filters = securityFilters
                .Where(x => x != null)
                .ToList();

            if (additionalFilter != null)
            {
                filters.Add(additionalFilter);
            }

            if (filters.Count == 0)
            {
                return null;
            }

            return filters.Aggregate(default(Expression<Func<TEntity, bool>>),
                (aggregate, filter) => aggregate.AndAlso(filter));
        }

        protected virtual IEnumerable<Expression<Func<TEntity, bool>>> GetSecurityFilters()
        {
            return null;
        }
    }
}