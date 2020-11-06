using System;
using System.Linq;
using System.Reflection.Metadata;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Implementations.JsonPatch;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Interfaces.Services.JsonPatch;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Core.Abstractions.Builders
{
    public abstract class EntityCrudBuilder<TKey, TEntity> : EntityBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private bool? _isActiveEntity;

        public bool IsActiveEntity 
        {
            get
            { 
                _isActiveEntity ??= typeof(IActiveEntity).IsAssignableFrom(EntityType);
                return _isActiveEntity.Value;
            }
        }

        private bool? _isTenantEntity;
        public bool IsTenantEntity
        {
            get
            {
                _isTenantEntity ??= EntityType.IsAssignableToGenericType(typeof(ITenantEntity<>));
                return _isTenantEntity.Value;
            }
        }
        private Type _tenantEntityKeyType;
        public Type TenantEntityKeyType { 
            get
            {
                if (_tenantEntityKeyType != null)
                    return _tenantEntityKeyType;

                if (!IsTenantEntity)
                {
                    return null;
                }

                _tenantEntityKeyType = EntityType.GetProperty(nameof(ITenantEntity<int>.TenantId))?.PropertyType;
                return _tenantEntityKeyType;
                
            }
        }

        private bool? _isModifiedEntity;
        

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

        public EntityCrudBuilder()
        {
            if (IsActiveEntity && IsModifiedEntity) {
                SearchRequestType = typeof(ActiveModifiedEntitySearchRequest);
            } else if (IsActiveEntity) {
                SearchRequestType = typeof(ActiveEntitySearchRequest);
            } else if (IsModifiedEntity) {
                SearchRequestType = typeof(ModifiedEntitySearchRequest);
            } else {
                SearchRequestType = typeof(EntitySearchRequest);
            }

            if (IsModifiedEntity)
            {
                var orderType = typeof(DefaultModifiedEntityDefaultOrderByProvider<,>).MakeGenericType(EntityKeyType, EntityType);
                WithRegistration<IEntityDefaultOrderByProvider<TKey, TEntity>>(orderType, false);
            }
            else
            {
                WithRegistration<IEntityDefaultOrderByProvider<TKey, TEntity>, DefaultEntityDefaultOrderByProvider<TKey, TEntity>>(false);
            }
            
            WithRegistration<IEntityDomainEventPublisher, DefaultEntityDomainEventPublisher>(false);
            WithRegistration<IDomainEventContextProvider, DefaultDomainEventContextProvider>(false);
            WithRegistration<IJsonPatchDocumentGenerator, JsonPatchDocumentDocumentGenerator>(false);
        }

        protected override void OnBuild()
        {
            base.OnBuild();
            ApplyPlatformTypes();
        }
    }
}