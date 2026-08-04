using System;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Web.Abstractions;
using Firebend.AutoCrud.ChangeTracking.Web.Implementations;
using Firebend.AutoCrud.ChangeTracking.Web.Implementations.Authorization;
using Firebend.AutoCrud.ChangeTracking.Web.Interfaces;
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
        where TVersion : class, IAutoCrudApiVersion
        => ResolveChangeTrackingControllerType(configurator, typeof(AbstractChangeTrackingReadController<,,,>));

    private static Type ResolveChangeTrackingControllerType<TBuilder, TKey, TEntity, TVersion>(
        ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator,
        Type openControllerType,
        params Type[] extraGenericArgs)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
    {
        var args = new Type[4 + extraGenericArgs.Length];
        args[0] = configurator.Builder.EntityKeyType;
        args[1] = configurator.Builder.EntityType;
        args[2] = typeof(TVersion);
        args[3] = configurator.ReadViewModelType;
        extraGenericArgs.CopyTo(args, 4);
        return openControllerType.MakeGenericType(args);
    }

    private static Type DefaultChangeTrackingViewModelType<TBuilder, TKey, TEntity, TVersion>(
        ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        => typeof(ChangeTrackingModel<,>).MakeGenericType(configurator.Builder.EntityKeyType, configurator.ReadViewModelType);

    public static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithChangeTrackingControllers<TBuilder, TKey,
        TEntity, TVersion>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator,
        string entityName = null,
        string entityNamePlural = null,
        string openApiName = null)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
    {
        var mapperType = typeof(IChangeTrackingViewModelMapper<,,,>)
            .MakeGenericType(configurator.Builder.EntityKeyType, configurator.Builder.EntityType, typeof(TVersion), configurator.ReadViewModelType);

        var defaultMapper = typeof(DefaultChangeTrackingViewModelMapper<,,,>)
            .MakeGenericType(configurator.Builder.EntityKeyType, configurator.Builder.EntityType, typeof(TVersion), configurator.ReadViewModelType);

        configurator.Builder.WithRegistration(mapperType, defaultMapper, mapperType, false);

        var controller = configurator.ChangeTrackingControllerType();
        return configurator.WithController(controller, controller, entityName, entityNamePlural, openApiName);
    }

    /// <summary>
    /// Adds a `/changes` controller for a custom <paramref name="changeTrackingEntityType"/> row type, mapped to
    /// <paramref name="changeTrackingViewModelType"/>. Shared by the tier-2 and tier-3 <c>WithChangeTrackingControllers</c>
    /// overloads.
    /// </summary>
    private static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithChangeTrackingControllersCore<TBuilder, TKey, TEntity, TVersion>(
        ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator,
        Type changeTrackingEntityType,
        Type changeTrackingViewModelType,
        string entityName,
        string entityNamePlural,
        string openApiName)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
    {
        var mapperType = typeof(IChangeTrackingViewModelMapper<,,,,,>)
            .MakeGenericType(configurator.Builder.EntityKeyType, configurator.Builder.EntityType, typeof(TVersion),
                configurator.ReadViewModelType, changeTrackingEntityType, changeTrackingViewModelType);

        var defaultMapper = typeof(DefaultChangeTrackingViewModelMapper<,,,,,>)
            .MakeGenericType(configurator.Builder.EntityKeyType, configurator.Builder.EntityType, typeof(TVersion),
                configurator.ReadViewModelType, changeTrackingEntityType, changeTrackingViewModelType);

        configurator.Builder.WithRegistration(mapperType, defaultMapper, mapperType, false);

        var controller = ResolveChangeTrackingControllerType(configurator, typeof(AbstractChangeTrackingReadController<,,,,,>),
            changeTrackingEntityType, changeTrackingViewModelType);

        return configurator.WithController(controller, controller, entityName, entityNamePlural, openApiName);
    }

    /// <summary>
    /// Adds a `/changes` controller for a given entity, for a custom <typeparamref name="TChangeTrackingEntity"/>
    /// row type. The response DTO defaults to <see cref="ChangeTrackingModel{TKey,TViewModel}"/> (no extra columns
    /// exposed) — use the 6-arg overload of this method to also expose a custom DTO with the row's extra columns.
    /// </summary>
    /// <param name="configurator">
    /// The <see cref="ControllerConfigurator{TBuilder,TKey,TEntity,TVersion}"/> to add the controller to.
    /// </param>
    /// <param name="entityName">Optional; the name to use for the entity in Swagger documentation.</param>
    /// <param name="entityNamePlural">Optional; the plural name to use for the entity in Swagger documentation.</param>
    /// <param name="openApiName">Optional; the OpenAPI group name to use for the controller.</param>
    /// <typeparam name="TBuilder">
    /// The type of <see cref="EntityCrudBuilder{TKey,TEntity}"/> builder.
    /// </typeparam>
    /// <typeparam name="TKey">
    /// The type of key the entity uses.
    /// </typeparam>
    /// <typeparam name="TEntity">
    /// The type of entity.
    /// </typeparam>
    /// <typeparam name="TVersion">
    /// The API version the controller is for.
    /// </typeparam>
    /// <typeparam name="TChangeTrackingEntity">
    /// The type of row persisted for each change. Must inherit <see cref="ChangeTrackingEntity{TKey,TEntity}"/>. Must match
    /// whatever <typeparamref name="TChangeTrackingEntity"/> was used when registering change tracking with
    /// <c>WithEfChangeTracking</c>/<c>WithMongoChangeTracking</c> for the same entity.
    /// </typeparam>
    /// <returns>
    /// A <see cref="ControllerConfigurator{TBuilder,TKey,TEntity,TVersion}"/>
    /// </returns>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithChangeTrackingControllers&lt;EntityCrudBuilder&lt;Guid, WeatherForecast&gt;, Guid, WeatherForecast, V1, WeatherForecastAuditRow&gt;()
    /// </code>
    /// </example>
    public static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithChangeTrackingControllers<TBuilder, TKey,
        TEntity, TVersion, TChangeTrackingEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator,
        string entityName = null,
        string entityNamePlural = null,
        string openApiName = null)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        where TChangeTrackingEntity : ChangeTrackingEntity<TKey, TEntity>
        => WithChangeTrackingControllersCore(configurator, typeof(TChangeTrackingEntity),
            DefaultChangeTrackingViewModelType(configurator), entityName, entityNamePlural, openApiName);

    /// <summary>
    /// Adds a `/changes` controller for a given entity, for a custom <typeparamref name="TChangeTrackingEntity"/> row
    /// type, mapped to a custom <typeparamref name="TChangeTrackingViewModel"/> DTO that exposes the row's extra
    /// columns.
    /// </summary>
    /// <param name="configurator">
    /// The <see cref="ControllerConfigurator{TBuilder,TKey,TEntity,TVersion}"/> to add the controller to.
    /// </param>
    /// <param name="entityName">Optional; the name to use for the entity in Swagger documentation.</param>
    /// <param name="entityNamePlural">Optional; the plural name to use for the entity in Swagger documentation.</param>
    /// <param name="openApiName">Optional; the OpenAPI group name to use for the controller.</param>
    /// <typeparam name="TBuilder">
    /// The type of <see cref="EntityCrudBuilder{TKey,TEntity}"/> builder.
    /// </typeparam>
    /// <typeparam name="TKey">
    /// The type of key the entity uses.
    /// </typeparam>
    /// <typeparam name="TEntity">
    /// The type of entity.
    /// </typeparam>
    /// <typeparam name="TVersion">
    /// The API version the controller is for.
    /// </typeparam>
    /// <typeparam name="TChangeTrackingEntity">
    /// The type of row persisted for each change. Must inherit <see cref="ChangeTrackingEntity{TKey,TEntity}"/>. Must match
    /// whatever <typeparamref name="TChangeTrackingEntity"/> was used when registering change tracking with
    /// <c>WithEfChangeTracking</c>/<c>WithMongoChangeTracking</c> for the same entity.
    /// </typeparam>
    /// <typeparam name="TChangeTrackingViewModel">
    /// The DTO returned by the `/changes` endpoint. Must inherit <c>ChangeTrackingModel&lt;TKey, TViewModel&gt;</c>, where
    /// <c>TViewModel</c> is the entity's own read view model. This constraint is enforced at runtime via
    /// <see cref="Type.MakeGenericType"/>, since <c>TViewModel</c> is only known here as <see cref="ControllerConfigurator{TBuilder,TKey,TEntity,TVersion}.ReadViewModelType"/>,
    /// not as a compile-time generic parameter of this method.
    /// </typeparam>
    /// <returns>
    /// A <see cref="ControllerConfigurator{TBuilder,TKey,TEntity,TVersion}"/>
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Throws if <typeparamref name="TChangeTrackingViewModel"/> does not inherit <c>ChangeTrackingModel&lt;TKey, TViewModel&gt;</c>
    /// for the entity's actual read view model type.
    /// </exception>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithChangeTrackingControllers&lt;EntityCrudBuilder&lt;Guid, WeatherForecast&gt;, Guid, WeatherForecast, V1, WeatherForecastAuditRow, WeatherForecastAuditViewModel&gt;()
    /// </code>
    /// </example>
    public static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithChangeTrackingControllers<TBuilder, TKey,
        TEntity, TVersion, TChangeTrackingEntity, TChangeTrackingViewModel>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator,
        string entityName = null,
        string entityNamePlural = null,
        string openApiName = null)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        where TChangeTrackingEntity : ChangeTrackingEntity<TKey, TEntity>
        where TChangeTrackingViewModel : class, new()
        => WithChangeTrackingControllersCore(configurator, typeof(TChangeTrackingEntity), typeof(TChangeTrackingViewModel),
            entityName, entityNamePlural, openApiName);

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
        where TVersion : class, IAutoCrudApiVersion
        => configurator.AddResourceAuthorization(configurator.ChangeTrackingControllerType(),
            typeof(EntityChangeTrackingAuthorizationFilter<TKey, TEntity, TVersion>), policy);

    /// <summary>
    /// Adds resource authorization to change tracking read requests for a custom <typeparamref name="TChangeTrackingEntity"/>
    /// row type, using the same <see cref="EntityChangeTrackingAuthorizationFilter{TKey,TEntity,TVersion}"/> as the
    /// default row type, since the filter only depends on the route's entity id argument name — not on the row or DTO type.
    /// </summary>
    /// <param name="policy">The resource authorization policy</param>
    /// <typeparam name="TBuilder">
    /// The type of <see cref="EntityCrudBuilder{TKey,TEntity}"/> builder.
    /// </typeparam>
    /// <typeparam name="TKey">
    /// The type of key the entity uses.
    /// </typeparam>
    /// <typeparam name="TEntity">
    /// The type of entity.
    /// </typeparam>
    /// <typeparam name="TVersion">
    /// The API version the controller is for.
    /// </typeparam>
    /// <typeparam name="TChangeTrackingEntity">
    /// The type of row persisted for each change. Must match whatever <see cref="WithChangeTrackingControllers{TBuilder,TKey,TEntity,TVersion,TChangeTrackingEntity}"/>
    /// was called with for the same entity, so the resolved controller type matches the one actually registered.
    /// </typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithChangeTrackingControllers&lt;EntityCrudBuilder&lt;Guid, WeatherForecast&gt;, Guid, WeatherForecast, V1, WeatherForecastAuditRow&gt;()
    ///          .AddChangeTrackingResourceAuthorization&lt;EntityCrudBuilder&lt;Guid, WeatherForecast&gt;, Guid, WeatherForecast, V1, WeatherForecastAuditRow&gt;()
    /// </code>
    /// </example>
    public static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddChangeTrackingResourceAuthorization<TBuilder,
        TKey, TEntity, TVersion, TChangeTrackingEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator,
        string policy = ChangeTrackingAuthorizationRequirement.DefaultPolicy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        where TChangeTrackingEntity : ChangeTrackingEntity<TKey, TEntity>
        => configurator.AddResourceAuthorization(
            ResolveChangeTrackingControllerType(configurator, typeof(AbstractChangeTrackingReadController<,,,,,>),
                typeof(TChangeTrackingEntity), DefaultChangeTrackingViewModelType(configurator)),
            typeof(EntityChangeTrackingAuthorizationFilter<TKey, TEntity, TVersion>), policy);

    /// <summary>
    /// Adds resource authorization to change tracking read requests for a custom <typeparamref name="TChangeTrackingEntity"/>
    /// row type mapped to a custom <typeparamref name="TChangeTrackingViewModel"/> DTO, using the same
    /// <see cref="EntityChangeTrackingAuthorizationFilter{TKey,TEntity,TVersion}"/> as the default row type, since the
    /// filter only depends on the route's entity id argument name — not on the row or DTO type.
    /// </summary>
    /// <param name="policy">The resource authorization policy</param>
    /// <typeparam name="TBuilder">
    /// The type of <see cref="EntityCrudBuilder{TKey,TEntity}"/> builder.
    /// </typeparam>
    /// <typeparam name="TKey">
    /// The type of key the entity uses.
    /// </typeparam>
    /// <typeparam name="TEntity">
    /// The type of entity.
    /// </typeparam>
    /// <typeparam name="TVersion">
    /// The API version the controller is for.
    /// </typeparam>
    /// <typeparam name="TChangeTrackingEntity">
    /// The type of row persisted for each change. Must match whatever <see cref="WithChangeTrackingControllers{TBuilder,TKey,TEntity,TVersion,TChangeTrackingEntity,TChangeTrackingViewModel}"/>
    /// was called with for the same entity, so the resolved controller type matches the one actually registered.
    /// </typeparam>
    /// <typeparam name="TChangeTrackingViewModel">
    /// The DTO returned by the `/changes` endpoint. Must match whatever <see cref="WithChangeTrackingControllers{TBuilder,TKey,TEntity,TVersion,TChangeTrackingEntity,TChangeTrackingViewModel}"/>
    /// was called with for the same entity, so the resolved controller type matches the one actually registered.
    /// </typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithChangeTrackingControllers&lt;EntityCrudBuilder&lt;Guid, WeatherForecast&gt;, Guid, WeatherForecast, V1, WeatherForecastAuditRow, WeatherForecastAuditViewModel&gt;()
    ///          .AddChangeTrackingResourceAuthorization&lt;EntityCrudBuilder&lt;Guid, WeatherForecast&gt;, Guid, WeatherForecast, V1, WeatherForecastAuditRow, WeatherForecastAuditViewModel&gt;()
    /// </code>
    /// </example>
    public static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddChangeTrackingResourceAuthorization<TBuilder,
        TKey, TEntity, TVersion, TChangeTrackingEntity, TChangeTrackingViewModel>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator,
        string policy = ChangeTrackingAuthorizationRequirement.DefaultPolicy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        where TChangeTrackingEntity : ChangeTrackingEntity<TKey, TEntity>
        where TChangeTrackingViewModel : class, new()
        => configurator.AddResourceAuthorization(
            ResolveChangeTrackingControllerType(configurator, typeof(AbstractChangeTrackingReadController<,,,,,>),
                typeof(TChangeTrackingEntity), typeof(TChangeTrackingViewModel)),
            typeof(EntityChangeTrackingAuthorizationFilter<TKey, TEntity, TVersion>), policy);

    public static IServiceCollection AddDefaultChangeTrackingResourceAuthorizationRequirement(this IServiceCollection serviceCollection)
        => serviceCollection.AddAuthorization(options =>
            {
                options.AddPolicy(ChangeTrackingAuthorizationRequirement.DefaultPolicy,
                    policy => policy.Requirements.Add(new ChangeTrackingAuthorizationRequirement()));
            });

    public static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddChangeTrackingAuthorizationPolicy<TBuilder, TKey, TEntity, TVersion>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator, string policy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        => configurator.AddAuthorizationPolicy(configurator.ChangeTrackingControllerType(), policy);

    /// <summary>
    /// Adds an authorization policy to change tracking read requests for a custom <typeparamref name="TChangeTrackingEntity"/>
    /// row type.
    /// </summary>
    /// <param name="policy">The authorization policy name</param>
    /// <typeparam name="TBuilder">
    /// The type of <see cref="EntityCrudBuilder{TKey,TEntity}"/> builder.
    /// </typeparam>
    /// <typeparam name="TKey">
    /// The type of key the entity uses.
    /// </typeparam>
    /// <typeparam name="TEntity">
    /// The type of entity.
    /// </typeparam>
    /// <typeparam name="TVersion">
    /// The API version the controller is for.
    /// </typeparam>
    /// <typeparam name="TChangeTrackingEntity">
    /// The type of row persisted for each change. Must match whatever <see cref="WithChangeTrackingControllers{TBuilder,TKey,TEntity,TVersion,TChangeTrackingEntity}"/>
    /// was called with for the same entity, so the resolved controller type matches the one actually registered.
    /// </typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithChangeTrackingControllers&lt;EntityCrudBuilder&lt;Guid, WeatherForecast&gt;, Guid, WeatherForecast, V1, WeatherForecastAuditRow&gt;()
    ///          .AddChangeTrackingAuthorizationPolicy&lt;EntityCrudBuilder&lt;Guid, WeatherForecast&gt;, Guid, WeatherForecast, V1, WeatherForecastAuditRow&gt;("Policy")
    /// </code>
    /// </example>
    public static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddChangeTrackingAuthorizationPolicy<TBuilder, TKey, TEntity, TVersion, TChangeTrackingEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator, string policy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        where TChangeTrackingEntity : ChangeTrackingEntity<TKey, TEntity>
        => configurator.AddAuthorizationPolicy(
            ResolveChangeTrackingControllerType(configurator, typeof(AbstractChangeTrackingReadController<,,,,,>),
                typeof(TChangeTrackingEntity), DefaultChangeTrackingViewModelType(configurator)),
            policy);

    /// <summary>
    /// Adds an authorization policy to change tracking read requests for a custom <typeparamref name="TChangeTrackingEntity"/>
    /// row type mapped to a custom <typeparamref name="TChangeTrackingViewModel"/> DTO.
    /// </summary>
    /// <param name="policy">The authorization policy name</param>
    /// <typeparam name="TBuilder">
    /// The type of <see cref="EntityCrudBuilder{TKey,TEntity}"/> builder.
    /// </typeparam>
    /// <typeparam name="TKey">
    /// The type of key the entity uses.
    /// </typeparam>
    /// <typeparam name="TEntity">
    /// The type of entity.
    /// </typeparam>
    /// <typeparam name="TVersion">
    /// The API version the controller is for.
    /// </typeparam>
    /// <typeparam name="TChangeTrackingEntity">
    /// The type of row persisted for each change. Must match whatever <see cref="WithChangeTrackingControllers{TBuilder,TKey,TEntity,TVersion,TChangeTrackingEntity,TChangeTrackingViewModel}"/>
    /// was called with for the same entity, so the resolved controller type matches the one actually registered.
    /// </typeparam>
    /// <typeparam name="TChangeTrackingViewModel">
    /// The DTO returned by the `/changes` endpoint. Must match whatever <see cref="WithChangeTrackingControllers{TBuilder,TKey,TEntity,TVersion,TChangeTrackingEntity,TChangeTrackingViewModel}"/>
    /// was called with for the same entity, so the resolved controller type matches the one actually registered.
    /// </typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithChangeTrackingControllers&lt;EntityCrudBuilder&lt;Guid, WeatherForecast&gt;, Guid, WeatherForecast, V1, WeatherForecastAuditRow, WeatherForecastAuditViewModel&gt;()
    ///          .AddChangeTrackingAuthorizationPolicy&lt;EntityCrudBuilder&lt;Guid, WeatherForecast&gt;, Guid, WeatherForecast, V1, WeatherForecastAuditRow, WeatherForecastAuditViewModel&gt;("Policy")
    /// </code>
    /// </example>
    public static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddChangeTrackingAuthorizationPolicy<TBuilder, TKey, TEntity, TVersion, TChangeTrackingEntity, TChangeTrackingViewModel>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator, string policy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        where TChangeTrackingEntity : ChangeTrackingEntity<TKey, TEntity>
        where TChangeTrackingViewModel : class, new()
        => configurator.AddAuthorizationPolicy(
            ResolveChangeTrackingControllerType(configurator, typeof(AbstractChangeTrackingReadController<,,,,,>),
                typeof(TChangeTrackingEntity), typeof(TChangeTrackingViewModel)),
            policy);
}
