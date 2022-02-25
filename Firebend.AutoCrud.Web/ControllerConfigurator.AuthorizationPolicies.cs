using System;
using System.Reflection.Emit;
using Firebend.AutoCrud.Web.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web;

public partial class ControllerConfigurator<TBuilder, TKey, TEntity>
{
    public (Type attributeType, CustomAttributeBuilder attributeBuilder) DefaultAuthorizationPolicy
    {
        get;
        private set;
    }

    public bool HasDefaultAuthorizationPolicy => DefaultAuthorizationPolicy != default
                                                 && DefaultAuthorizationPolicy.attributeBuilder != null
                                                 && DefaultAuthorizationPolicy.attributeType != null;


    /// <summary>
    /// Adds an authorization policy to requests for an entity that use the specified controller
    /// </summary>
    /// <param name="type">The type of the controller to add the policy for</param>
    /// <param name="authorizePolicy">Optional; the authorization policy name</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddAuthorizationPolicy()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddAuthorizationPolicy(Type type,
        string authorizePolicy = "")
    {
        var (attributeType, attributeBuilder) = GetAuthorizationAttributeInfo(authorizePolicy);
        Builder.WithAttribute(type, attributeType, attributeBuilder);
        return this;
    }

    /// <summary>
    /// Adds an authorization policy to requests for an entity that use the specified controller
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
    ///          .AddAuthorizationPolicy<Controller>("Policy")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddAuthorizationPolicy<TController>(string policy)
        => AddAuthorizationPolicy(typeof(TController), policy);

    /// <summary>
    /// Adds an authorization policy to Create requests using the abstract create controller
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
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddCreateAuthorizationPolicy(string policy)
        => AddAuthorizationPolicy(typeof(AbstractEntityCreateController<,,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, CreateViewModelType, ReadViewModelType),
            policy);

    /// <summary>
    /// Adds an authorization policy to DELETE requests using the abstract delete controller
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
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddDeleteAuthorizationPolicy(string policy)
        => AddAuthorizationPolicy(typeof(AbstractEntityDeleteController<,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, ReadViewModelType), policy);

    /// <summary>
    /// Adds an authorization policy to GET requests using the abstract read controller
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
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddReadAuthorizationPolicy(string policy)
        => AddAuthorizationPolicy(typeof(AbstractEntityReadController<,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, ReadViewModelType), policy);

    /// <summary>
    /// Adds an authorization policy to GET `/all` requests using the abstract read all controller
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
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddReadAllAuthorizationPolicy(string policy)
        => AddAuthorizationPolicy(typeof(AbstractEntityReadAllController<,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, ReadViewModelType), policy);

    /// <summary>
    /// Adds an authorization policy to search requests using the abstract search controller
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
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddSearchAuthorizationPolicy(string policy)
    {
        var type = typeof(AbstractEntitySearchController<,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, Builder.SearchType, ReadViewModelType);

        return AddAuthorizationPolicy(type, policy);
    }

    /// <summary>
    /// Adds an authorization policy to PUT requests using the abstract update controller
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
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddUpdateAuthorizationPolicy(string policy)
        => AddAuthorizationPolicy(typeof(AbstractEntityUpdateController<,,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, UpdateViewModelType, ReadViewModelType),
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
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddAlterAuthorizationPolicies(string policy = "")
    {
        AddCreateAuthorizationPolicy(policy);
        AddDeleteAuthorizationPolicy(policy);
        AddUpdateAuthorizationPolicy(policy);

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
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddQueryAuthorizationPolicies(string policy = "")
    {
        AddReadAuthorizationPolicy(policy);
        AddReadAllAuthorizationPolicy(policy);
        AddSearchAuthorizationPolicy(policy);

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
    ///          .AddAuthorizationPolicies("Policy")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddAuthorizationPolicies(string policy = "")
    {
        DefaultAuthorizationPolicy = GetAuthorizationAttributeInfo(policy);

        AddAttributeToAllControllers(DefaultAuthorizationPolicy.attributeType,
            DefaultAuthorizationPolicy.attributeBuilder);

        return this;
    }

    private static (Type attributeType, CustomAttributeBuilder attributeBuilder) GetAuthorizationAttributeInfo(
        string authorizePolicy = "")
    {
        var authType = typeof(AuthorizeAttribute);

        var authCtor = authorizePolicy == null
            ? null
            : authType.GetConstructor(!string.IsNullOrWhiteSpace(authorizePolicy)
                ? new[] {typeof(string)}
                : Type.EmptyTypes);

        if (authCtor == null)
        {
            return default;
        }

        var args = !string.IsNullOrWhiteSpace(authorizePolicy)
            ? new object[] {authorizePolicy}
            : new object[] { };

        return (authType, new CustomAttributeBuilder(authCtor, args));
    }
}
