using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web;

public partial class ControllerConfigurator<TBuilder, TKey, TEntity>
{
    /// <summary>
    /// Adds resource authorization requirements to requests for an entity that use the specified controller
    /// </summary>
    /// <param name="type">The type of the controller to add the authorization for</param>
    /// <param name="requirements">The authorization requirements</param>
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
        IEnumerable<IAuthorizationRequirement> requirements)
    {
        var (attributeType, attributeBuilder) = GetResourceAuthorizationAttributeInfo(filterType, requirements);
        Builder.WithAttribute(type, attributeType, attributeBuilder);
        return this;
    }

    /// <summary>
    /// Adds resource authorization to Create requests using the abstract create controller
    /// </summary>
    /// <param name="requirements">The authorization requirements</param>
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
        IEnumerable<IAuthorizationRequirement> requirements = null)
        => AddResourceAuthorization(typeof(AbstractEntityCreateController<,,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, CreateViewModelType, ReadViewModelType),
            typeof(AbstractEntityCreateAuthorizationFilter<TKey, TEntity>), requirements ?? new []{ new CreateAuthorizationRequirement() });

    /// <summary>
    /// Adds resource authorization to DELETE requests using the abstract delete controller
    /// </summary>
    /// <param name="requirements">The authorization requirements</param>
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
        IEnumerable<IAuthorizationRequirement> requirements = null)
        => AddResourceAuthorization(typeof(AbstractEntityDeleteController<,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, ReadViewModelType),
            typeof(AbstractEntityDeleteAuthorizationFilter<TKey, TEntity>), requirements ?? new []{ new DeleteAuthorizationRequirement() });

    /// <summary>
    /// Adds resource authorization to GET requests using the abstract read controller
    /// </summary>
    /// <param name="requirements">The authorization requirements</param>
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
        IEnumerable<IAuthorizationRequirement> requirements = null)
        => AddResourceAuthorization(typeof(AbstractEntityReadController<,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, ReadViewModelType),
            typeof(AbstractEntityReadAuthorizationFilter<TKey, TEntity>), requirements ?? new []{ new ReadAuthorizationRequirement() });

    /// <summary>
    /// Adds resource authorization to GET `/all` requests using the abstract read all controller
    /// </summary>
    /// <param name="requirements">The authorization requirements</param>
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
        IEnumerable<IAuthorizationRequirement> requirements = null)
        => AddResourceAuthorization(typeof(AbstractEntityReadAllController<,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, ReadViewModelType),
            typeof(AbstractEntityReadAllAuthorizationFilter<TKey, TEntity>), requirements ?? new []{ new ReadAllAuthorizationRequirement() });

    /// <summary>
    /// Adds resource authorization to PUT requests using the abstract update controller
    /// </summary>
    /// <param name="requirements">The authorization requirements</param>
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
        IEnumerable<IAuthorizationRequirement> requirements = null)
        => AddResourceAuthorization(typeof(AbstractEntityUpdateController<,,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, ReadViewModelType, UpdateViewModelType),
            typeof(AbstractEntityUpdateAuthorizationFilter<TKey, TEntity>), requirements ?? new []{ new UpdateAuthorizationRequirement() });

    /// <summary>
    /// Adds resource authorization to all requests that modify an entity (Create, Update, and Delete) and use the abstract controllers
    /// </summary>
    /// <param name="requirements">The authorization requirements</param>
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
        IEnumerable<IAuthorizationRequirement> requirements = null)
    {
        var authorizationRequirements = requirements as IAuthorizationRequirement[] ?? requirements?.ToArray();
        AddCreateResourceAuthorization(authorizationRequirements);
        AddDeleteResourceAuthorization(authorizationRequirements);
        AddUpdateResourceAuthorization(authorizationRequirements);

        return this;
    }

    /// <summary>
    /// Adds resource authorization to all requests that read an entity (Read, Read all, and Search) and use the abstract controllers
    /// </summary>
    /// <param name="requirements">The authorization requirements</param>
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
        IEnumerable<IAuthorizationRequirement> requirements = null)
    {
        var authorizationRequirements = requirements as IAuthorizationRequirement[] ?? requirements?.ToArray();
        AddReadResourceAuthorization(authorizationRequirements);
        AddReadAllResourceAuthorization(authorizationRequirements);

        return this;
    }

    private static (Type attributeType, CustomAttributeBuilder attributeBuilder) GetResourceAuthorizationAttributeInfo(
        Type authType,
        IEnumerable<IAuthorizationRequirement> requirements)
    {
        var authCtor = authType.GetConstructor(new[] {typeof(IEnumerable<IAuthorizationRequirement>)});

        if (authCtor == null)
        {
            return default;
        }

        var args = new object[] {requirements};

        return (authType, new CustomAttributeBuilder(authCtor, args));
    }
}
