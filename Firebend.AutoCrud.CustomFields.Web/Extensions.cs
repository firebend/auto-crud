using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.CustomFields.Web.Abstractions;
using Firebend.AutoCrud.CustomFields.Web.Implementations.Authorization;
using Firebend.AutoCrud.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.CustomFields.Web;

public static class Extensions
{
    public static Type CustomFieldsCreateControllerType<TBuilder, TKey, TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        => typeof(AbstractCustomFieldsCreateController<,>)
            .MakeGenericType(configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType);

    public static Type CustomFieldsUpdateControllerType<TBuilder, TKey, TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        => typeof(AbstractCustomFieldsUpdateController<,>)
            .MakeGenericType(configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType);

    public static Type CustomFieldsDeleteControllerType<TBuilder, TKey, TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        => typeof(AbstractCustomFieldsDeleteController<,>)
            .MakeGenericType(configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType);

    public static Type CustomFieldsSearchControllerType<TBuilder, TKey, TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        => typeof(AbstractCustomFieldsSearchController<,>)
            .MakeGenericType(configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType);

    public static ControllerConfigurator<TBuilder, TKey, TEntity> WithCustomFieldsControllers<TBuilder, TKey, TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator,
        string entityName = null,
        string entityNamePlural = null,
        string openApiName = null)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        configurator.Builder
            .WithRegistration<ICustomFieldsValidationService<TKey, TEntity>,
                DefaultCustomFieldsValidationService<TKey, TEntity>>(false);

        var createController = configurator.CustomFieldsCreateControllerType();
        configurator.WithController(createController, createController, entityName, entityNamePlural, openApiName);

        var updateController = configurator.CustomFieldsUpdateControllerType();
        configurator.WithController(updateController, updateController, entityName, entityNamePlural, openApiName);

        var deleteController = configurator.CustomFieldsDeleteControllerType();
        configurator.WithController(deleteController, deleteController, entityName, entityNamePlural, openApiName);

        var searchController = configurator.CustomFieldsSearchControllerType();
        configurator.WithController(searchController, searchController, entityName, entityNamePlural, openApiName);

        return configurator;
    }

    /// <summary>
    /// Adds resource authorization to change tracking read requests using the abstract change tracking controller
    /// </summary>
    /// <param name="policy">The resource authorization policy</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithChangeTrackingControllers()
    ///          .AddChangeTrackingResourceAuthorization()
    /// </code>
    /// </example>
    public static ControllerConfigurator<TBuilder, TKey, TEntity> AddCustomFieldsResourceAuthorization<TBuilder,
        TKey,
        TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator,
        string policy = CustomFieldsAuthorizationRequirement.DefaultPolicy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey> =>
        configurator
            .AddResourceAuthorization(configurator.CustomFieldsCreateControllerType(),
                typeof(CustomFieldsAuthorizationFilter<TKey, TEntity>), policy)
            .AddResourceAuthorization(configurator.CustomFieldsUpdateControllerType(),
                typeof(CustomFieldsAuthorizationFilter<TKey, TEntity>), policy)
            .AddResourceAuthorization(configurator.CustomFieldsDeleteControllerType(),
                typeof(CustomFieldsAuthorizationFilter<TKey, TEntity>), policy);

    public static IServiceCollection AddDefaultCustomFieldsResourceAuthorizationRequirement(
        this IServiceCollection serviceCollection)
    {
        serviceCollection.AddAuthorization(options =>
        {
            options.AddPolicy(CustomFieldsAuthorizationRequirement.DefaultPolicy,
                policy => policy.Requirements.Add(new CustomFieldsAuthorizationRequirement()));
        });

        return serviceCollection;
    }

    public static ControllerConfigurator<TBuilder, TKey, TEntity> AddCustomFieldsCreateAuthorizationPolicy<TBuilder,
        TKey, TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator, string policy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        => configurator.AddAuthorizationPolicy(configurator.CustomFieldsCreateControllerType(), policy);

    public static ControllerConfigurator<TBuilder, TKey, TEntity> AddCustomFieldsUpdateAuthorizationPolicy<TBuilder,
        TKey, TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator, string policy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        => configurator.AddAuthorizationPolicy(configurator.CustomFieldsUpdateControllerType(), policy);

    public static ControllerConfigurator<TBuilder, TKey, TEntity> AddCustomFieldsDeleteAuthorizationPolicy<TBuilder,
        TKey, TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator, string policy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        => configurator.AddAuthorizationPolicy(configurator.CustomFieldsDeleteControllerType(), policy);

    public static ControllerConfigurator<TBuilder, TKey, TEntity> AddCustomFieldsSearchAuthorizationPolicy<TBuilder,
        TKey, TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator, string policy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        => configurator.AddAuthorizationPolicy(configurator.CustomFieldsSearchControllerType(), policy);

    public static ControllerConfigurator<TBuilder, TKey, TEntity> AddCustomFieldsQueryAuthorizationPolicies<TBuilder,
        TKey, TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator, string policy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        => configurator.AddCustomFieldsSearchAuthorizationPolicy(policy);

    public static ControllerConfigurator<TBuilder, TKey, TEntity> AddCustomFieldsAlterAuthorizationPolicies<TBuilder,
        TKey, TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator, string policy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        => configurator.AddCustomFieldsCreateAuthorizationPolicy(policy)
            .AddCustomFieldsUpdateAuthorizationPolicy(policy)
            .AddCustomFieldsDeleteAuthorizationPolicy(policy);

    public static ControllerConfigurator<TBuilder, TKey, TEntity> AddAuthorizationPolicy<TBuilder, TKey, TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator, string policy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        => configurator
            .AddCustomFieldsQueryAuthorizationPolicies(policy)
            .AddCustomFieldsAlterAuthorizationPolicies(policy);
}
