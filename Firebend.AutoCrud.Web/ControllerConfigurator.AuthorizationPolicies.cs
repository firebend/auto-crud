using System;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web;

public partial class ControllerConfigurator<TBuilder, TKey, TEntity, TVersion>
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
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddAuthorizationPolicy(Type type,
        string authorizePolicy = "")
    {
        var (attributeType, attributeBuilder) = GetAuthorizationAttributeInfo(authorizePolicy);
        Builder.WithAttribute(type, attributeType, attributeBuilder, true);
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
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddAuthorizationPolicy<TController>(string policy)
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
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddCreateAuthorizationPolicy(string policy)
        => AddAuthorizationPolicy(CreateControllerType(), policy);

    /// <summary>
    /// Adds an authorization policy to Create (multiple) requests using the abstract create multiple controller
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
    ///          .AddCreateMultipleAuthorizationPolicy("Policy")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddCreateMultipleAuthorizationPolicy(string policy)
        => AddAuthorizationPolicy(CreateMultipleControllerType(), policy);

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
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddDeleteAuthorizationPolicy(string policy)
        => AddAuthorizationPolicy(DeleteControllerType(), policy)
            .AddAuthorizationPolicy(UndoDeleteControllerType(), policy);

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
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddReadAuthorizationPolicy(string policy)
        => AddAuthorizationPolicy(ReadControllerType(), policy);

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
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddReadAllAuthorizationPolicy(string policy)
        => AddAuthorizationPolicy(ReadAllControllerType(), policy);

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
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddSearchAuthorizationPolicy(string policy) =>
        AddAuthorizationPolicy(SearchControllerType(), policy);

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
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddUpdateAuthorizationPolicy(string policy)
        => AddAuthorizationPolicy(UpdateControllerType(), policy);

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
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddAlterAuthorizationPolicies(string policy = "")
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
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddQueryAuthorizationPolicies(string policy = "")
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
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddAuthorizationPolicies(string policy = "")
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
                ? new[] { typeof(string) }
                : Type.EmptyTypes);

        if (authCtor == null)
        {
            return default;
        }

        var args = !string.IsNullOrWhiteSpace(authorizePolicy)
            ? new object[] { authorizePolicy }
            : new object[] { };

        return (authType, new CustomAttributeBuilder(authCtor, args));
    }
}
