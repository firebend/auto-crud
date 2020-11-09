using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public abstract class MongoCreateClient<TKey, TEntity> : MongoClientBaseEntity<TKey, TEntity>, IMongoCreateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IEntityDomainEventPublisher _eventPublisher;
        private readonly IDomainEventContextProvider _domainEventContextProvider;

        protected MongoCreateClient(IMongoClient client,
            ILogger<MongoCreateClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            IEntityDomainEventPublisher eventPublisher,
            IDomainEventContextProvider domainEventContextProvider) : base(client, logger, entityConfiguration)
        {
            _eventPublisher = eventPublisher;
            _domainEventContextProvider = domainEventContextProvider;
        }

        public async virtual Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var mongoCollection = GetCollection();

            if (entity is IModifiedEntity modified)
            {
                var now = DateTimeOffset.Now;
                modified.CreatedDate = now;
                modified.ModifiedDate = now;
            }

            await RetryErrorAsync(() => mongoCollection.InsertOneAsync(entity, null, cancellationToken))
                .ConfigureAwait(false);

            await PublishDomainEventAsync(entity, cancellationToken).ConfigureAwait(false);

            return entity;
        }
        
        private Task PublishDomainEventAsync(TEntity savedEntity, CancellationToken cancellationToken = default)
        {
            if (_eventPublisher != null && !(_eventPublisher is DefaultEntityDomainEventPublisher))
            {
                var domainEvent = new EntityAddedDomainEvent<TEntity>
                {
                    Entity = savedEntity,
                    EventContext = _domainEventContextProvider?.GetContext()
                };

                return _eventPublisher.PublishEntityAddEventAsync(domainEvent, cancellationToken);
            }
            
            return Task.CompletedTask;
        }
    }
    
    public abstract class MongoTenantCreateClient<TKey, TEntity, TTenantKey> : MongoCreateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ITenantEntity<TTenantKey>, new()
        where TTenantKey: struct
    {
        private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

        protected MongoTenantCreateClient(IMongoClient client, ILogger<MongoCreateClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration, IEntityDomainEventPublisher eventPublisher,
            IDomainEventContextProvider domainEventContextProvider, 
            ITenantEntityProvider<TTenantKey> tenantEntityProvider) : base(client, logger, entityConfiguration,
            eventPublisher, domainEventContextProvider)
        {
            _tenantEntityProvider = tenantEntityProvider;
        }

        protected override async Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken)
        {
            var tenant = await _tenantEntityProvider
                .GetTenantAsync(cancellationToken)
                .ConfigureAwait(false);

            Expression<Func<TEntity, bool>> tenantFilter = x => x.TenantId.Equals(tenant.TenantId);

            return new[] {tenantFilter};
        }

        public override async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantEntityProvider
                .GetTenantAsync(cancellationToken)
                .ConfigureAwait(false);

            entity.TenantId = tenant.TenantId;
            return await base.CreateAsync(entity, cancellationToken).ConfigureAwait(false);
        }
    }
}