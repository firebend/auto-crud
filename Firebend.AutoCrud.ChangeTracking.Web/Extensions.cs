using System;
using Firebend.AutoCrud.ChangeTracking.Web.Abstractions;
using Firebend.AutoCrud.ChangeTracking.Web.Implementations.Authorization;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.ChangeTracking.Web;

public static class Extensions
{
    public static Type ChangeTrackingControllerType<TBuilder, TKey, TEntity, TVersion>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IApiVersion
        => typeof(AbstractChangeTrackingReadController<,,,>)
            .MakeGenericType(configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType,
                typeof(TVersion),
                configurator.ReadViewModelType);

    public static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithChangeTrackingControllers<TBuilder, TKey,
        TEntity, TVersion>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator,
        string entityName = null,
        string entityNamePlural = null,
        string openApiName = null)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IApiVersion
    {
        var controller = configurator.ChangeTrackingControllerType();
        return configurator.WithController(controller, controller, entityName, entityNamePlural, openApiName);
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
    public static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddChangeTrackingResourceAuthorization<TBuilder,
        TKey,
        TEntity, TVersion>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator,
        string policy = ChangeTrackingAuthorizationRequirement.DefaultPolicy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IApiVersion
        => configurator.AddResourceAuthorization(configurator.ChangeTrackingControllerType(),
            typeof(EntityChangeTrackingAuthorizationFilter<TKey, TEntity, TVersion>), policy);

    public static IServiceCollection AddDefaultChangeTrackingResourceAuthorizationRequirement(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddAuthorization(options =>
        {
            options.AddPolicy(ChangeTrackingAuthorizationRequirement.DefaultPolicy,
                policy => policy.Requirements.Add(new ChangeTrackingAuthorizationRequirement()));
        });

        return serviceCollection;
    }

    public static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddChangeTrackingAuthorizationPolicy<TBuilder, TKey, TEntity, TVersion>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator, string policy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IApiVersion
        => configurator.AddAuthorizationPolicy(configurator.ChangeTrackingControllerType(),
            policy);
}
