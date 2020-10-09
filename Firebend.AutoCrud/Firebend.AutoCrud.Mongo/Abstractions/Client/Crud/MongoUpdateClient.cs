using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public class MongoUpdateClient<TEntity, TKey> : MongoClientBaseEntity<TKey, TEntity>, IMongoUpdateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IMongoCollectionKeyGenerator<TKey, TEntity> _keyGenerator;
        
        public MongoUpdateClient(IMongoClient client,
            ILogger<MongoUpdateClient<TEntity, TKey>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            IMongoCollectionKeyGenerator<TKey, TEntity> keyGenerator) : base(client, logger, entityConfiguration)
        {
            _keyGenerator = keyGenerator;
        }
        
        protected virtual async Task<TEntity> UpdateInternalAsync(TEntity entity,
            Expression<Func<TEntity, bool>> filter,
            bool doUpsert,
            CancellationToken cancellationToken)
        {
            var filters = BuildFilters(filter);

            var mongoCollection = GetCollection();

            var original = await RetryErrorAsync(() =>
                mongoCollection.FindOneAndReplaceAsync(
                    Builders<TEntity>.Filter.Where(filters),
                    entity,
                    new FindOneAndReplaceOptions<TEntity, TEntity>
                    {
                        ReturnDocument = ReturnDocument.Before,
                        IsUpsert = doUpsert
                    },
                    cancellationToken));

            if (original != null)
            {
                //todo: domain events
                // var patch = _jsonPatchDocumentGenerator.Generate(original, entity);
                //
                // await PublishUpdateAsync(patch, original, cancellationToken).ConfigureAwait(false);
                //
                // return entity;
            }

            // if (doUpsert)
            // {
            //     await PublishCreateAsync(entity, cancellationToken).ConfigureAwait(false);
            //     return entity;
            // }

            return null;
        }

        protected virtual async Task<List<TKey>> UpdateManyInternalAsync(IMongoCollection<TEntity> mongoCollection,
            List<EntityUpdate<TEntity>> entities,
            CancellationToken cancellationToken = default)
        {
            if (entities == null || !entities.Any())
            {
                throw new ArgumentException("There are no entities provided to update.", nameof(entities));
            }

            var ids = new List<TKey>();

            foreach (var entityUpdate in entities)
            {
                var id = await mongoCollection
                    .AsQueryable()
                    .Where(entityUpdate.Filter)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (id.Equals(default))
                {
                    id = await _keyGenerator.GenerateKeyAsync(cancellationToken);
                }

                entityUpdate.Entity.Id = id;

                ids.Add(id);
            }

            var writes = entities
                .Select(x => new ReplaceOneModel<TEntity>(Builders<TEntity>
                    .Filter
                    .Where(y => y.Id.Equals(x.Entity.Id)), x.Entity)
                    {
                        IsUpsert = true,
                    });

            await RetryErrorAsync(() => mongoCollection.BulkWriteAsync(
                writes,
                new BulkWriteOptions
                {
                    IsOrdered = true
                }, cancellationToken));

            return ids;
        }

        public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return UpdateInternalAsync(entity, x => x.Id.Equals(entity.Id), false, cancellationToken);
        }

        public Task<TEntity> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return UpdateInternalAsync(entity, x => x.Id.Equals(entity.Id), true, cancellationToken);
        }

        public Task<TEntity> UpsertAsync(TEntity entity, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            return UpdateInternalAsync(entity, filter, true, cancellationToken);
        }

        public async Task<List<TEntity>> UpsertManyAsync(List<EntityUpdate<TEntity>> entities, CancellationToken cancellationToken = default)
        {
            var collection = GetCollection();

            var ids = await UpdateManyInternalAsync(collection, entities, cancellationToken)
                .ConfigureAwait(false);

            if (ids.Any())
            {
                var filter = Builders<TEntity>.Filter.In(x => x.Id, ids);

                var updatedEntities = await RetryErrorAsync(() => collection
                    .AsQueryable()
                    .Where(_ => filter.Inject())
                    .ToListAsync(cancellationToken))
                    .ConfigureAwait(false);

                return updatedEntities;
            }

            return new List<TEntity>();
        }

        public async Task<List<TOut>> UpsertManyAsync<TOut>(List<EntityUpdate<TEntity>> entities, Expression<Func<TEntity, TOut>> projection, CancellationToken cancellationToken = default)
        {
            var collection = GetCollection();

            var ids = await UpdateManyInternalAsync(collection, entities, cancellationToken)
                .ConfigureAwait(false);

            if (ids.Any())
            {
                var filter = Builders<TEntity>.Filter.In(x => x.Id, ids);

                var updatedEntities = await RetryErrorAsync(() => collection
                    .AsQueryable()
                    .Where(_ => filter.Inject())
                    .Select(projection)
                    .ToListAsync(cancellationToken))
                    .ConfigureAwait(false);

                return updatedEntities;
            }

            return new List<TOut>();
        }

        public async Task<TEntity> UpdateAsync(TKey id, JsonPatchDocument<TEntity> patch, CancellationToken cancellationToken = default)
        {
            var mongoCollection = GetCollection();
            
            var filter = BuildFilters(x => x.Id.Equals(id));
            
            var entity = await mongoCollection
                .AsQueryable()
                .Where(filter)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
            {
                return null;
            }

            patch.ApplyTo(entity);

            return await UpdateInternalAsync(entity, x => x.Id.Equals(entity.Id), false, cancellationToken);
        }
    }
}