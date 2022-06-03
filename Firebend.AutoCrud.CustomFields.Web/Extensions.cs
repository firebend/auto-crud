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

        var createController = typeof(AbstractCustomFieldsCreateController<,>)
            .MakeGenericType(configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType);

        configurator.WithController(createController, createController, entityName, entityNamePlural, openApiName);

        var updateController = typeof(AbstractCustomFieldsUpdateController<,>)
            .MakeGenericType(configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType);

        configurator.WithController(updateController, updateController, entityName, entityNamePlural, openApiName);

        var deleteController = typeof(AbstractCustomFieldsDeleteController<,>)
            .MakeGenericType(configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType);

        configurator.WithController(deleteController, deleteController, entityName, entityNamePlural, openApiName);

        var searchController = typeof(AbstractCustomFieldsSearchController<,>)
            .MakeGenericType(configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType);

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
        where TEntity : class, IEntity<TKey>
    {
        var createController = typeof(AbstractCustomFieldsCreateController<,>)
            .MakeGenericType(configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType);

        configurator.AddResourceAuthorization(createController,
            typeof(CustomFieldsAuthorizationFilter<TKey, TEntity>), policy);

        var updateController = typeof(AbstractCustomFieldsUpdateController<,>)
            .MakeGenericType(configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType);

        configurator.AddResourceAuthorization(updateController,
            typeof(CustomFieldsAuthorizationFilter<TKey, TEntity>), policy);

        var deleteController = typeof(AbstractCustomFieldsDeleteController<,>)
            .MakeGenericType(configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType);

        configurator.AddResourceAuthorization(deleteController,
            typeof(CustomFieldsAuthorizationFilter<TKey, TEntity>), policy);

        return configurator;
    }

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
}
