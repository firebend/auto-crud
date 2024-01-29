using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;
using Firebend.AutoCrud.EntityFramework.Abstractions.Entities;
using Firebend.AutoCrud.EntityFramework.ExceptionHandling;
using Firebend.AutoCrud.EntityFramework.Including;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework;

/// <summary>
/// Encapsulates logic to configure custom fields.
/// </summary>
/// <typeparam name="TBuilder">
/// The type of builder custom fields is being configured for.
/// </typeparam>
/// <typeparam name="TKey">
/// The entity's key type.
/// </typeparam>
/// <typeparam name="TEntity">
/// The entity type.
/// </typeparam>
public class EfCustomFieldsConfigurator<TBuilder, TKey, TEntity> : EntityCrudConfigurator<TBuilder, TKey, TEntity>
    where TBuilder : EntityFrameworkEntityBuilder<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>, new()
{
    public EfCustomFieldsConfigurator(TBuilder builder) : base(builder)
    {
        if (builder.DbContextType is null)
        {
            throw new Exception("Please configure the builders db context");
        }
    }

    /// <summary>
    /// Configures custom fields for this entity.
    /// </summary>
    /// <param name="configure">
    ///  A call back to add any additional configurations. This could be change tracking, web controllers, etc.
    /// </param>
    /// <returns>
    /// The EF Custom Fields Configurator
    /// </returns>
    public EfCustomFieldsConfigurator<TBuilder, TKey, TEntity> AddCustomFields(Action<EntityFrameworkEntityBuilder<Guid, EfCustomFieldsModel<TKey, TEntity>>> configure = null)
    {
        AddCustomFields(Builder, false);

        if (configure == null)
        {
            return this;
        }

        var customFieldsBuilder = new EntityFrameworkEntityBuilder<Guid, EfCustomFieldsModel<TKey, TEntity>>
        {
            SignatureBase = $"{typeof(TEntity).Name}_CustomFields",
            DbContextType = Builder.DbContextType,
        };
        configure(customFieldsBuilder);
        Builder.Registrations.Add(typeof(object), [
            new BuilderRegistration { Builder = customFieldsBuilder }
        ]);

        return this;
    }

    /// <summary>
    /// Configures custom fields for this entity. Use this variation of AddCustomFields when
    /// each custom field record should have the tenant key associated to it.
    /// </summary>
    /// <param name="configure">
    ///  A call back to add any additional configurations. This could be change tracking, web controllers, etc.
    /// </param>
    /// <typeparam name="TTenantKey">The type corresponding to the entity's tenant key.</typeparam>
    /// <returns>
    /// The EF Custom Fields Configurator
    /// </returns>
    public EfCustomFieldsConfigurator<TBuilder, TKey, TEntity> AddCustomFieldsTenant<TTenantKey>(
        Action<EntityFrameworkEntityBuilder<Guid, EfCustomFieldsModelTenant<TKey, TEntity, TTenantKey>>> configure = null)
        where TTenantKey : struct
    {
        AddCustomFields(Builder, true);

        if (configure == null)
        {
            return this;
        }

        var customFieldsBuilder = new EntityFrameworkEntityBuilder<Guid, EfCustomFieldsModelTenant<TKey, TEntity, TTenantKey>>
        {
            SignatureBase = $"{typeof(TEntity).Name}_CustomFields",
            DbContextType = Builder.DbContextType,
        };
        configure(customFieldsBuilder);
        Builder.Registrations.Add(typeof(object), [
            new BuilderRegistration { Builder = customFieldsBuilder }
        ]);

        return this;
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private static TBuilder AddCustomFields(
        TBuilder builder,
        bool isTenantEntity)
    {
        var guidType = typeof(Guid);

        var efModelType = isTenantEntity
            ? typeof(EfCustomFieldsModelTenant<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, builder.TenantEntityKeyType)
            : typeof(EfCustomFieldsModel<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType);

        builder.WithRegistration(
            typeof(IDbContextProvider<,>).MakeGenericType(guidType, efModelType),
            typeof(AbstractCustomFieldsDbContextProvider<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
            false);

        builder.WithRegistration(
            typeof(IDbContextOptionsProvider<,>).MakeGenericType(guidType, efModelType),
            typeof(AbstractCustomFieldsDbContextOptionsProvider<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
            false);

        builder.WithRegistration(
            typeof(IDbContextConnectionStringProvider<,>).MakeGenericType(guidType, efModelType),
            typeof(AbstractCustomFieldsConnectionStringProvider<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
            false);

        builder.WithRegistration(
            typeof(IEntityFrameworkIncludesProvider<,>).MakeGenericType(guidType, efModelType),
            typeof(DefaultEntityFrameworkIncludesProvider<,>).MakeGenericType(guidType, efModelType),
            false);

        builder.WithRegistration(
            typeof(IEntityFrameworkDbUpdateExceptionHandler<,>).MakeGenericType(guidType, efModelType),
            typeof(DefaultEntityFrameworkDbUpdateExceptionHandler<,>).MakeGenericType(guidType, efModelType),
            false);

        builder.WithRegistration<ICustomFieldsCreateService<TKey, TEntity>>(
            typeof(AbstractEfCustomFieldsCreateService<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
            false);

        builder.WithRegistration(
            typeof(IEntityFrameworkCreateClient<,>).MakeGenericType(guidType, efModelType),
            isTenantEntity
                ? typeof(EntityFrameworkTenantCreateClient<,,>).MakeGenericType(guidType, efModelType, builder.TenantEntityKeyType)
                : typeof(EntityFrameworkCreateClient<,>).MakeGenericType(guidType, efModelType),
            false);

        builder.WithRegistration<ICustomFieldsUpdateService<TKey, TEntity>>(
            typeof(AbstractEfCustomFieldsUpdateService<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
            false);

        builder.WithRegistration(
            typeof(IEntityFrameworkUpdateClient<,>).MakeGenericType(guidType, efModelType),
            isTenantEntity
                ? typeof(EntityFrameworkTenantUpdateClient<,,>).MakeGenericType(guidType, efModelType, builder.TenantEntityKeyType)
                : typeof(EntityFrameworkUpdateClient<,>).MakeGenericType(guidType, efModelType),
            false);

        builder.WithRegistration<ICustomFieldsDeleteService<TKey, TEntity>>(
            typeof(AbstractEfCustomFieldsDeleteService<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
            false);

        builder.WithRegistration(
            typeof(IEntityFrameworkDeleteClient<,>).MakeGenericType(guidType, efModelType),
            isTenantEntity
                ? typeof(EntityFrameworkTenantDeleteClient<,,>).MakeGenericType(guidType, efModelType, builder.TenantEntityKeyType)
                : typeof(EntityFrameworkDeleteClient<,>).MakeGenericType(guidType, efModelType),
            false);

        builder.WithRegistration<ICustomFieldsSearchService<TKey, TEntity>>(
            typeof(AbstractEfCustomFieldSearchService<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
            false);

        builder.WithRegistration(
            typeof(IEntityFrameworkQueryClient<,>).MakeGenericType(guidType, efModelType),
            isTenantEntity
                ? typeof(EntityFrameworkTenantQueryClient<,,>).MakeGenericType(guidType, efModelType, builder.TenantEntityKeyType)
                : typeof(EntityFrameworkQueryClient<,>).MakeGenericType(guidType, efModelType),
            false);

        builder.WithRegistration<ICustomFieldsReadService<TKey, TEntity>>(
            typeof(AbstractEfCustomFieldsReadService<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
            false);

        builder.WithRegistration(
            typeof(IEntityReadService<,>).MakeGenericType(guidType, efModelType),
            typeof(EntityFrameworkEntityReadService<,>).MakeGenericType(guidType, efModelType),
            false);

        builder.WithRegistration(
            typeof(IEntityQueryOrderByHandler<,>).MakeGenericType(guidType, efModelType),
            typeof(DefaultEntityQueryOrderByHandler<,>).MakeGenericType(guidType, efModelType),
            false);

        return builder;
    }

    public EfCustomFieldsConfigurator<TBuilder, TKey, TEntity> WithSearchHandler<TService>()
    {
        var serviceType = typeof(TService);
        var registrationType = typeof(IEntitySearchHandler<TKey, TEntity, CustomFieldsSearchRequest>);
        Builder.WithRegistration(registrationType, serviceType,
            registrationType);
        return this;
    }
}
