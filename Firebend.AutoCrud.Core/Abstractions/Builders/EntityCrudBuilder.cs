using System;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Implementations.Entities;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.JsonPatch;

namespace Firebend.AutoCrud.Core.Abstractions.Builders
{
    public abstract class EntityCrudBuilder<TKey, TEntity> : EntityBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private bool? _isActiveEntity;

        private bool? _isModifiedEntity;

        private bool? _isTenantEntity;

        private Type _tenantEntityKeyType;

        protected EntityCrudBuilder()
        {
            if (IsActiveEntity && IsModifiedEntity)
            {
                SearchRequestType = typeof(ActiveModifiedEntitySearchRequest);
            }
            else if (IsActiveEntity)
            {
                SearchRequestType = typeof(ActiveEntitySearchRequest);
            }
            else if (IsModifiedEntity)
            {
                SearchRequestType = typeof(ModifiedEntitySearchRequest);
            }
            else
            {
                SearchRequestType = typeof(EntitySearchRequest);
            }

            WithRegistration<IEntityDomainEventPublisher, DefaultEntityDomainEventPublisher>(false);
            WithRegistration<IDomainEventContextProvider, DefaultDomainEventContextProvider>(false);
            WithRegistration<IJsonPatchGenerator, JsonPatchGenerator>(false);
            WithRegistration<IEntityQueryOrderByHandler<TKey, TEntity>, DefaultEntityQueryOrderByHandler<TKey, TEntity>>(false);
            WithRegistration<IEntityTransactionOutbox, InMemoryEntityTransactionOutbox>(false);

            if (IsModifiedEntity)
            {
                WithRegistration<IDefaultEntityOrderByProvider<TKey, TEntity>>(typeof(DefaultEntityOrderByProviderModified<,>).MakeGenericType(EntityKeyType, EntityType));
            }
            else if (IsActiveEntity)
            {
                WithRegistration<IDefaultEntityOrderByProvider<TKey, TEntity>>(typeof(DefaultEntityOrderByProviderActive<,>).MakeGenericType(EntityKeyType, EntityType));
            }
            else
            {
                WithRegistrationInstance<IDefaultEntityOrderByProvider<TKey, TEntity>>(new DefaultDefaultEntityOrderByProvider<TKey, TEntity>());
            }
        }

        public bool IsActiveEntity
        {
            get
            {
                _isActiveEntity ??= typeof(IActiveEntity).IsAssignableFrom(EntityType);
                return _isActiveEntity.Value;
            }
        }

        public bool IsTenantEntity
        {
            get
            {
                _isTenantEntity ??= EntityType.IsAssignableToGenericType(typeof(ITenantEntity<>));
                return _isTenantEntity.Value;
            }
        }

        public Type TenantEntityKeyType
        {
            get
            {
                if (_tenantEntityKeyType != null)
                {
                    return _tenantEntityKeyType;
                }

                if (!IsTenantEntity)
                {
                    return null;
                }

                _tenantEntityKeyType = EntityType.GetProperty(nameof(ITenantEntity<int>.TenantId))
                    ?.PropertyType;
                return _tenantEntityKeyType;
            }
        }

        public bool IsModifiedEntity
        {
            get
            {
                _isModifiedEntity ??= typeof(IModifiedEntity).IsAssignableFrom(EntityType);
                return _isModifiedEntity.Value;
            }
        }

        public abstract Type CreateType { get; }

        public abstract Type ReadType { get; }

        public abstract Type SearchType { get; }

        public abstract Type UpdateType { get; }

        public abstract Type DeleteType { get; }

        public Type SearchRequestType { get; set; }

        protected abstract void ApplyPlatformTypes();

        protected override void OnBuild()
        {
            base.OnBuild();
            ApplyPlatformTypes();
        }
    }
}
