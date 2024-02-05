using System;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations.Concurrency;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Implementations.DomainEvents;
using Firebend.AutoCrud.Core.Implementations.Entities;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Abstractions.Builders;

public abstract class EntityCrudBuilder<TKey, TEntity> : EntityBuilder<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private bool? _isActiveEntity;

    private bool? _isModifiedEntity;

    private bool? _isTenantEntity;

    private Type _tenantEntityKeyType;

    protected EntityCrudBuilder(IServiceCollection services) : base(services)
    {
        SearchRequestType = IsActiveEntity switch
        {
            true when IsModifiedEntity => typeof(ActiveModifiedEntitySearchRequest),
            true => typeof(ActiveEntitySearchRequest),
            _ => IsModifiedEntity ? typeof(ModifiedEntitySearchRequest) : typeof(EntitySearchRequest)
        };

        WithRegistration<IEntityDomainEventPublisher<TKey, TEntity>, DefaultEntityDomainEventPublisher<TKey, TEntity>>(false);
        WithRegistration<IDomainEventPublisherService<TKey, TEntity>, DefaultDomainEventPublisherService<TKey, TEntity>>(false);
        WithRegistration<IDomainEventContextProvider, DefaultDomainEventContextProvider>(false);
        WithRegistration<IEntityQueryOrderByHandler<TKey, TEntity>, DefaultEntityQueryOrderByHandler<TKey, TEntity>>(false);
        WithRegistration<IEntityTransactionOutbox, InMemoryEntityTransactionOutbox>(false);
        WithRegistration<IDistributedLockService, DistributedLockService>(false);

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
