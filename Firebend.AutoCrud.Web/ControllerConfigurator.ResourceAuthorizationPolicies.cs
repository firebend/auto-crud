using System;
using System.Reflection.Emit;
using Firebend.AutoCrud.Web.Abstractions;

namespace Firebend.AutoCrud.Web;

public partial class ControllerConfigurator<TBuilder, TKey, TEntity>
{
    /// <summary>
    /// Adds a resource authorization policy to requests for an entity that use the specified controller
    /// </summary>
    /// <param name="type">The type of the controller to add the policy for</param>
    /// <param name="policy">Optional; the authorization policy name</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddResourceAuthorizationPolicy()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddResourceAuthorizationPolicy(Type type,
        string policy = "")
    {
        var (attributeType, attributeBuilder) = GetResourceAuthorizationAttributeInfo(typeof(string), policy);
        Builder.WithAttribute(type, attributeType, attributeBuilder);
        return this;
    }

    /// <summary>
    /// Adds a resource authorization policy to requests for an entity that use the specified controller
    /// </summary>
    /// <typeparam name="TController">The type of the controller to add the authorization policy to</typeparam>
    /// <param name="policy">The authorization policy name</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddResourceAuthorizationPolicy<Controller>("Policy")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddResourceAuthorizationPolicy<TController>(string policy)
        => AddResourceAuthorizationPolicy(typeof(TController), policy);

    /// <summary>
    /// Adds a resource authorization policy to Create requests using the abstract create controller
    /// </summary>
    /// <param name="policy">The authorization policy name</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddCreateAuthorizationPolicy("Policy")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddCreateResourceAuthorizationPolicy(string policy)
        => AddResourceAuthorizationPolicy(typeof(AbstractEntityCreateController<,,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, CreateViewModelType, ReadViewModelType),
            policy);

    /// <summary>
    /// Adds a resource authorization policy to DELETE requests using the abstract delete controller
    /// </summary>
    /// <param name="policy">The authorization policy name</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddDeleteAuthorizationPolicy("Policy")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddDeleteResourceAuthorizationPolicy(string policy)
        => AddResourceAuthorizationPolicy(typeof(AbstractEntityDeleteController<,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, ReadViewModelType), policy);

    /// <summary>
    /// Adds a resource authorization policy to GET requests using the abstract read controller
    /// </summary>
    /// <param name="policy">The authorization policy name</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddReadAuthorizationPolicy("Policy")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddReadResourceAuthorizationPolicy(string policy)
        => AddResourceAuthorizationPolicy(typeof(AbstractEntityReadController<,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, ReadViewModelType), policy);

    /// <summary>
    /// Adds a resource authorization policy to GET `/all` requests using the abstract read all controller
    /// </summary>
    /// <param name="policy">The authorization policy name</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddReadAllAuthorizationPolicy("Policy")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddReadAllResourceAuthorizationPolicy(string policy)
        => AddResourceAuthorizationPolicy(typeof(AbstractEntityReadAllController<,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, ReadViewModelType), policy);

    /// <summary>
    /// Adds a resource authorization policy to search requests using the abstract search controller
    /// </summary>
    /// <param name="policy">The authorization policy name</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddSearchAuthorizationPolicy("Policy")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddSearchResourceAuthorizationPolicy(string policy)
    {
        var type = typeof(AbstractEntitySearchController<,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, Builder.SearchType, ReadViewModelType);

        return AddResourceAuthorizationPolicy(type, policy);
    }

    /// <summary>
    /// Adds a resource authorization policy to PUT requests using the abstract update controller
    /// </summary>
    /// <param name="policy">The authorization policy name</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddUpdateAuthorizationPolicy("Policy")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddUpdateResourceAuthorizationPolicy(string policy)
        => AddResourceAuthorizationPolicy(typeof(AbstractEntityUpdateController<,,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, ReadViewModelType, UpdateViewModelType),
            policy);

    /// <summary>
    /// Adds an authorization policies to all requests that modify an entity (Create, Update, and Delete) and use the abstract controllers
    /// </summary>
    /// <param name="policy">The authorization policy name</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddAlterAuthorizationPolicies("Policy")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddAlterResourceAuthorizationPolicies(string policy = "")
    {
        AddCreateResourceAuthorizationPolicy(policy);
        AddDeleteResourceAuthorizationPolicy(policy);
        AddUpdateResourceAuthorizationPolicy(policy);

        return this;
    }

    /// <summary>
    /// Adds an authorization policies to all requests that read an entity (Read, Read all, and Search) and use the abstract controllers
    /// </summary>
    /// <param name="policy">The authorization policy name</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddQueryAuthorizationPolicies("Policy")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddQueryResourceAuthorizationPolicies(string policy = "")
    {
        AddReadResourceAuthorizationPolicy(policy);
        AddReadAllResourceAuthorizationPolicy(policy);
        AddSearchResourceAuthorizationPolicy(policy);

        return this;
    }

    /// <summary>
    /// Adds an authorization policies to all requests that use the abstract controllers
    /// </summary>
    /// <param name="policy">The authorization policy name</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddResourceAuthorizationPolicies("Policy")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddResourceAuthorizationPolicies(string policy = "")
    {
        // TODO
        return this;
    }

    private static (Type attributeType, CustomAttributeBuilder attributeBuilder) GetResourceAuthorizationAttributeInfo(
        Type authType,
        string policy = "")
    {
        var authCtor = policy == null
            ? null
            : authType.GetConstructor(!string.IsNullOrWhiteSpace(policy)
                ? new[] {typeof(string)}
                : Type.EmptyTypes);

        if (authCtor == null)
        {
            return default;
        }

        var args = !string.IsNullOrWhiteSpace(policy)
            ? new object[] {policy}
            : new object[] { };

        return (authType, new CustomAttributeBuilder(authCtor, args));
    }
}
