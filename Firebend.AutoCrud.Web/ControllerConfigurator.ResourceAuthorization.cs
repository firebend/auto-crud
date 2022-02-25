using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;

namespace Firebend.AutoCrud.Web;

public partial class ControllerConfigurator<TBuilder, TKey, TEntity>
{
    /// <summary>
    /// Adds resource authorization requirements to requests for an entity that use the specified controller
    /// </summary>
    /// <param name="type">The type of the controller to add the authorization for</param>
    /// <param name="policy">The resource authorization policy</param>
    /// <param name="viewModelType">Type of view model for controller request body</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddResourceAuthorization(new (){ MyRequirement })
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddResourceAuthorization(Type type, Type filterType,
        string policy,
        string[] propertyNames = null,
        object[] propertyValues = null)
    {
        var (attributeType, attributeBuilder) =
            GetResourceAuthorizationAttributeInfo(filterType, policy, propertyNames, propertyValues);
        Builder.WithAttribute(type, attributeType, attributeBuilder);
        return this;
    }

    /// <summary>
    /// Adds resource authorization to Create requests using the abstract create controller
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
    ///          .AddCreateResourceAuthorization(new []{ MyRequirement })
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddCreateResourceAuthorization(
        string policy = CreateAuthorizationRequirement.DefaultPolicy)
        => AddResourceAuthorization(typeof(AbstractEntityCreateController<,,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, CreateViewModelType, ReadViewModelType),
            typeof(AbstractEntityCreateAuthorizationFilter), policy,
            AbstractEntityCreateAuthorizationFilter.RequiredProperties, new object[] {CreateViewModelType});

    /// <summary>
    /// Adds resource authorization to DELETE requests using the abstract delete controller
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
    ///          .AddDeleteResourceAuthorization(new []{ MyRequirement })
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddDeleteResourceAuthorization(
        string policy = DeleteAuthorizationRequirement.DefaultPolicy)
        => AddResourceAuthorization(typeof(AbstractEntityDeleteController<,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, ReadViewModelType),
            typeof(AbstractEntityDeleteAuthorizationFilter<TKey, TEntity>), policy);

    /// <summary>
    /// Adds resource authorization to GET requests using the abstract read controller
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
    ///          .AddReadResourceAuthorization(new []{ MyRequirement })
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddReadResourceAuthorization(
        string policy = ReadAuthorizationRequirement.DefaultPolicy)
        => AddResourceAuthorization(typeof(AbstractEntityReadController<,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, ReadViewModelType),
            typeof(AbstractEntityReadAuthorizationFilter), policy);

    /// <summary>
    /// Adds resource authorization to GET `/all` requests using the abstract read all controller
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
    ///          .AddReadAllResourceAuthorization(new []{ MyRequirement })
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddReadAllResourceAuthorization(
        string policy = ReadAllAuthorizationRequirement.DefaultPolicy)
        => AddResourceAuthorization(typeof(AbstractEntityReadAllController<,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, ReadViewModelType),
            typeof(AbstractEntityReadAllAuthorizationFilter), policy);

    /// <summary>
    /// Adds resource authorization to PUT requests using the abstract update controller
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
    ///          .AddUpdateResourceAuthorization(new []{ MyRequirement })
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddUpdateResourceAuthorization(
        string policy = UpdateAuthorizationRequirement.DefaultPolicy) =>
        AddResourceAuthorization(typeof(AbstractEntityUpdateController<,,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, UpdateViewModelType, ReadViewModelType),
            typeof(AbstractEntityUpdateAuthorizationFilter<TKey, TEntity>), policy,
            AbstractEntityUpdateAuthorizationFilter<TKey, TEntity>.RequiredProperties, new object[] {UpdateViewModelType});

    /// <summary>
    /// Adds resource authorization to all requests that modify an entity (Create, Update, and Delete) and use the abstract controllers
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
    ///          .AddAlterResourceAuthorization(new []{ MyRequirement })
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddAlterResourceAuthorization(
        string policy)
    {
        AddCreateResourceAuthorization(policy);
        AddDeleteResourceAuthorization(policy);
        AddUpdateResourceAuthorization(policy);

        return this;
    }

    /// <summary>
    /// Adds resource authorization to all requests that read an entity (Read, Read all, and Search) and use the abstract controllers
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
    ///          .AddQueryResourceAuthorization(new []{ MyRequirement })
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddQueryResourceAuthorization(
        string policy)
    {
        AddReadResourceAuthorization(policy);
        AddReadAllResourceAuthorization(policy);

        return this;
    }

    /// <summary>
    /// Add all resource authorization to all controllers
    /// </summary>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddResourceAuthorization("Policy")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddResourceAuthorization(string policy)
    {
        AddCreateResourceAuthorization(policy);
        AddDeleteResourceAuthorization(policy);
        AddUpdateResourceAuthorization(policy);
        AddReadResourceAuthorization(policy);
        AddReadAllResourceAuthorization(policy);

        return this;
    }

    /// <summary>
    /// Add all resource authorization to all controllers
    /// </summary>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddResourceAuthorization()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddResourceAuthorization()
    {
        AddCreateResourceAuthorization();
        AddDeleteResourceAuthorization();
        AddUpdateResourceAuthorization();
        AddReadResourceAuthorization();
        AddReadAllResourceAuthorization();

        return this;
    }

    private (Type attributeType, CustomAttributeBuilder attributeBuilder) GetResourceAuthorizationAttributeInfo(
        Type filterType,
        string policy,
        string[] propertyNames,
        object[] propertyValues)
    {
        var authCtor = filterType.GetConstructor(new[] {typeof(string)});

        if (authCtor == null)
        {
            return default;
        }

        var args = new object[] {policy};

        if (propertyNames == null || propertyValues == null)
        {
            return (filterType,
                new CustomAttributeBuilder(authCtor, args));
        }

        var propertyInfos = GetPropertyInfos(filterType, propertyNames);

        return (filterType,
            new CustomAttributeBuilder(authCtor, args, propertyInfos, propertyValues));
    }

    private PropertyInfo[] GetPropertyInfos(Type filterType, string[] propertyNames)
    {
        var propertyInfos = propertyNames.Select(filterType.GetProperty);
        return propertyInfos.ToArray();
    }
}
