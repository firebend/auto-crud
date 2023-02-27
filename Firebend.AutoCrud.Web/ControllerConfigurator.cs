// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Implementations.Paging;
using Firebend.AutoCrud.Web.Implementations.ViewModelMappers;
using Firebend.AutoCrud.Web.Interfaces;
using Firebend.AutoCrud.Web.Models;
using Humanizer;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web;

public static class ControllerConfiguratorCache
{
    public static bool IsSwaggerApplied { get; set; }
}

public partial class
    ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> : EntityBuilderConfigurator<TBuilder, TKey, TEntity>
    where TBuilder : EntityCrudBuilder<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
    where TVersion : class, IApiVersion
{
    internal ControllerConfigurator(TBuilder builder) : base(builder)
    {
        if (builder.EntityType == null)
        {
            throw new ArgumentException("Please configure an entity type for this builder first.", nameof(builder));
        }

        if (builder.EntityKeyType == null)
        {
            throw new ArgumentException("Please configure an entity key type for this entity first.", nameof(builder));
        }

        var name = (string.IsNullOrWhiteSpace(Builder.EntityName)
            ? Builder.EntityType.Name
            : Builder.EntityName).Humanize(LetterCasing.Title);

        OpenApiEntityName = name;
        OpenApiEntityNamePlural = name.Pluralize();

        WithRoute($"/api/v1/{name.Kebaberize()}");
        WithOpenApiGroupName(name);

        WithValidationService<DefaultEntityValidationService<TKey, TEntity, TVersion>>(false);

        Builder.WithRegistration<IEntityKeyParser<TKey, TEntity, TVersion>, DefaultEntityKeyParser<TKey, TEntity, TVersion>>(false);

        WithCreateViewModel<DefaultCreateUpdateViewModel<TKey, TEntity>, DefaultCreateViewModelMapper<TKey, TEntity, TVersion>>();
        WithReadViewModel<TEntity, DefaultReadViewModelMapper<TKey, TEntity, TVersion>>();
        WithUpdateViewModel<DefaultCreateUpdateViewModel<TKey, TEntity>, TEntity, DefaultUpdateViewModelMapper<TKey, TEntity, TVersion>>();
        WithCreateMultipleViewModel<MultipleEntityViewModel<TEntity>, TEntity,
            DefaultCreateMultipleViewModelMapper<TKey, TEntity, TVersion>>();
        WithSearchRequest();
        WithMaxPageSize();
    }

    public string Route { get; private set; }

    private (Type attributeType, CustomAttributeBuilder attributeBuilder) GetRouteAttributeInfo()
    {
        var routeType = typeof(RouteAttribute);
        var routeCtor = routeType.GetConstructor(new[] { typeof(string) });

        if (routeCtor == null)
        {
            return default;
        }

        var attributeBuilder = new CustomAttributeBuilder(routeCtor, new object[] { Route });

        return (routeType, attributeBuilder);
    }

    private void AddRouteAttribute(Type controllerType)
    {
        var (routeType, attributeBuilder) = GetRouteAttributeInfo();

        Builder.WithAttribute(controllerType, routeType, attributeBuilder);
    }

    /// <summary>
    /// Registers a given controller
    /// </summary>
    /// <param name="type"></param>
    /// <param name="typeToCheck"></param>
    /// <param name="genericArgs"></param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithController())
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithController(Type type,
        Type typeToCheck,
        string entityName = null,
        string entityNamePlural = null,
        string openApiName = null,
        params Type[] genericArgs)
    {
        var hasGenerics = genericArgs != null && genericArgs.Length > 0;
        var registrationType = hasGenerics ? type.MakeGenericType(genericArgs) : type;

        var typeToCheckGeneric = hasGenerics ? typeToCheck.MakeGenericType(genericArgs) : typeToCheck;

        if (!typeToCheckGeneric.IsAssignableFrom(registrationType))
        {
            throw new Exception($"Registration type {registrationType} is not assignable to {typeToCheckGeneric}");
        }

        Builder.WithRegistration(registrationType, registrationType);

        AddRouteAttribute(registrationType);
        AddOpenApiGroupNameAttribute(registrationType, openApiName);
        AddOpenApiEntityNameAttribute(registrationType, entityName, entityNamePlural);

        if (HasDefaultAuthorizationPolicy)
        {
            Builder.WithAttribute(registrationType, DefaultAuthorizationPolicy.attributeType,
                DefaultAuthorizationPolicy.attributeBuilder);
        }

        return this;
    }

    /// <summary>
    /// Registers a given controller
    /// </summary>
    /// <typeparam name="TTypeCheck"></typeparam>
    /// <param name="type"></param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithController<>())
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithController<TTypeCheck>(Type type)
        => WithController(type, typeof(TTypeCheck));

    /// <summary>
    /// Registers a given controller
    /// </summary>
    /// <typeparam name="TController"></typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithController<>())
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithController<TController>()
        => WithController(typeof(TController), typeof(TController));

    private void AddAttributeToAllControllers(Type attributeType, CustomAttributeBuilder attributeBuilder) =>
        GetRegisteredControllers()
            .ToList()
            .ForEach(x => Builder.WithAttribute(x.Key, attributeType, attributeBuilder));

    private IEnumerable<KeyValuePair<Type, Registration>> GetRegisteredControllers() => Builder
        .Registrations
        .SelectMany(x => x.Value, (pair, registration) => new KeyValuePair<Type, Registration>(pair.Key, registration))
        .Where(x => x.Value is ServiceRegistration)
        .Where(x => typeof(AbstractEntityControllerBase<TVersion>).IsAssignableFrom((x.Value as ServiceRegistration)?.ServiceType));

    /// <summary>
    /// Specifies the base route to use for an entity
    /// </summary>
    /// <param name="route"></param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers(true)
    ///          .WithOpenApiGroupName("Weather Forecasts")
    ///          .WithRoute("api/v1/mongo-person"))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithRoute(string route)
    {
        route ??= OpenApiEntityName.Kebaberize();
        Route = route;
        var (aType, aBuilder) = GetRouteAttributeInfo();
        AddAttributeToAllControllers(aType, aBuilder);
        return this;
    }

    // TODO TS: fix these docs
    /// <summary>
    /// Specifies the base route to use for an entity
    /// </summary>
    /// <param name="route"></param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers(true)
    ///          .WithOpenApiGroupName("Weather Forecasts")
    ///          .WithRoute("api/v1/mongo-person"))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithVersionedRoute(string route = null, string routePrefix = "api")
    {
        route ??= OpenApiEntityName.Kebaberize();
        Route = $"{routePrefix}/v{{version:apiVersion}}/{route}".TrimUrlExtraSlashes();
        var (aType, aBuilder) = GetRouteAttributeInfo();
        AddAttributeToAllControllers(aType, aBuilder);
        return this;
    }

    private ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithControllerHelper(
        Type controllerType,
        Type mapperInterfaceType,
        Type registrationType,
        Type viewModelType,
        Type viewModelMapper,
        bool makeRegistrationTypeGeneric = false,
        bool makeControllerTypeGeneric = true)
    {
        Type mapper;
        Type controller;

        if (makeControllerTypeGeneric)
        {
            controller = controllerType
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), viewModelType);

            mapper = mapperInterfaceType
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), viewModelType);
        }
        else
        {
            controller = controllerType;
            mapper = mapperInterfaceType;
        }

        if (makeRegistrationTypeGeneric)
        {
            registrationType = registrationType
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), viewModelType);
        }

        WithController(controller, registrationType);

        if (viewModelMapper != null)
        {
            Builder.WithRegistration(mapper, viewModelMapper, mapper);
        }

        return this;
    }

    public Type CreateControllerType()
        => typeof(AbstractEntityCreateController<,,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), CreateViewModelType, ReadViewModelType);

    public Type ValidateCreateControllerType()
        => typeof(AbstractEntityValidateCreateController<,,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), CreateViewModelType, ReadViewModelType);

    public Type CreateMultipleControllerType()
        => typeof(AbstractEntityCreateMultipleController<,,,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), CreateMultipleViewModelWrapperType,
                CreateMultipleViewModelType, ReadViewModelType);

    public Type ValidateUpdateControllerType()
        => typeof(AbstractEntityValidateUpdateController<,,,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), UpdateViewModelType, UpdateViewModelBodyType, ReadViewModelType);

    public Type UpdateControllerType()
        => typeof(AbstractEntityUpdateController<,,,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), UpdateViewModelType, UpdateViewModelBodyType, ReadViewModelType);

    public Type ReadControllerType()
        => typeof(AbstractEntityReadController<,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), ReadViewModelType);

    public Type ReadAllControllerType()
        => typeof(AbstractEntityReadAllController<,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), ReadViewModelType);

    public Type DeleteControllerType()
        => typeof(AbstractEntityDeleteController<,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), ReadViewModelType);

    public Type SearchControllerType()
        => typeof(AbstractEntitySearchController<,,,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), Builder.SearchRequestType, SearchViewModelType, ReadViewModelType);

    /// <summary>
    /// Registers a CREATE controller for the entity
    /// </summary>
    /// <param name="serviceType"></param>
    /// <param name="viewModelMapper"></param>
    /// <param name="resultModelTypeMapper"></param>
    /// <param name="makeServiceGeneric"></param>
    /// <param name="createControllerType"></param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithCreateController()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithCreateController(Type serviceType,
        Type viewModelMapper,
        Type resultModelTypeMapper,
        bool makeServiceGeneric,
        Func<Type> controllerType)
    {
        if (viewModelMapper != null)
        {
            var createViewModelMapperInterface = typeof(ICreateViewModelMapper<,,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityKeyType, typeof(TVersion), CreateViewModelType);

            Builder.WithRegistration(createViewModelMapperInterface, viewModelMapper, createViewModelMapperInterface);
        }

        if (resultModelTypeMapper != null)
        {
            var resultMapperInterface = typeof(IReadViewModelMapper<,,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityKeyType, typeof(TVersion), ReadViewModelType);

            Builder.WithRegistration(resultMapperInterface, resultModelTypeMapper, resultMapperInterface);
        }

        if (makeServiceGeneric)
        {
            serviceType = serviceType.MakeGenericType(Builder.EntityKeyType,
                Builder.EntityType,
                typeof(TVersion),
                CreateViewModelType,
                ReadViewModelType);
        }

        WithController(serviceType, controllerType());

        return this;
    }

    /// <summary>
    /// Registers a CREATE controller for the entity using a service type
    /// </summary>
    /// <typeparam name="TRegistrationType">The service type to use</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithCreateController<>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithCreateController<TRegistrationType>()
        => WithCreateController(typeof(TRegistrationType),
            null,
            null,
            false,
            CreateControllerType);

    /// <summary>
    /// Registers a CREATE controller for the entity using auto-generated types
    /// </summary>
    /// <param name="includeValidationController">Optional;  Whether to include the Validate controller</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithCreateController<>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithCreateController(bool includeValidationController = true)
    {
        if (includeValidationController)
        {
            WithValidateCreateController();
        }
        return WithCreateController(
            typeof(AbstractEntityCreateController<,,,,>),
            null,
            null,
            true,
            CreateControllerType);
    }

    /// <summary>
    /// Registers a Validate CREATE controller for the entity using a service type
    /// </summary>
    /// <typeparam name="TRegistrationType">The service type to use</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithValidateCreateController<>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithValidateCreateController<TRegistrationType>()
        => WithCreateController(typeof(TRegistrationType),
            null,
            null,
            false,
            ValidateCreateControllerType);

    /// <summary>
    /// Registers a Validate CREATE controller for the entity using auto-generated types
    /// </summary>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithValidateCreateController<>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithValidateCreateController()
        => WithCreateController(
            typeof(AbstractEntityValidateCreateController<,,,,>),
            null,
            null,
            true,
            ValidateCreateControllerType);

    /// <summary>
    /// Registers a DELETE controller for the entity
    /// </summary>
    /// <param name="registrationType"></param>
    /// <param name="viewModelMapper"></param>
    /// <param name="makeRegistrationTypeGeneric"></param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithDeleteController()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithDeleteController(Type registrationType,
        Type viewModelMapper = null,
        bool makeRegistrationTypeGeneric = false) => WithControllerHelper(
        typeof(AbstractEntityDeleteController<,,,>),
        typeof(ICreateViewModelMapper<,,,>),
        registrationType,
        ReadViewModelType,
        viewModelMapper,
        makeRegistrationTypeGeneric);

    /// <summary>
    /// Registers a DELETE controller for the entity using a registration type
    /// </summary>
    /// <typeparam name="TRegistrationType">The service type to use</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithDeleteController<>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithDeleteController<TRegistrationType>()
        => WithDeleteController(typeof(TRegistrationType));

    /// <summary>
    /// Registers a DELETE controller for the entity using auto-generated types
    /// </summary>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithDeleteController()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithDeleteController()
        => WithDeleteController(typeof(AbstractEntityDeleteController<,,,>),
            makeRegistrationTypeGeneric: true);

    /// <summary>
    /// Registers a GET `/all` controller for the entity
    /// </summary>
    /// <param name="registrationType"></param>
    /// <param name="viewModelMapper"></param>
    /// <param name="makeRegistrationTypeGeneric"></param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithGetAllController()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithGetAllController(Type registrationType,
        Type viewModelMapper = null,
        bool makeRegistrationTypeGeneric = false) => WithControllerHelper(
        typeof(AbstractEntityReadAllController<,,,>),
        typeof(ICreateViewModelMapper<,,,>),
        registrationType,
        ReadViewModelType,
        viewModelMapper,
        makeRegistrationTypeGeneric);

    /// <summary>
    /// Registers a GET `/all` controller for the entity using a registration type
    /// </summary>
    /// <typeparam name="TRegistrationType">The service type to use</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithGetAllController<>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithGetAllController<TRegistrationType>() =>
        WithGetAllController(typeof(TRegistrationType));

    /// <summary>
    /// Registers a GET `/all` controller for the entity using auto-generated types
    /// </summary>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithGetAllController()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithGetAllController()
        => WithGetAllController(typeof(AbstractEntityReadAllController<,,,>),
             makeRegistrationTypeGeneric: true);

    /// <summary>
    /// Registers a GET controller for the entity
    /// </summary>
    /// <param name="registrationType"></param>
    /// <param name="viewModelMapper"></param>
    /// <param name="makeRegistrationTypeGeneric"></param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithReadController()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithReadController(Type registrationType,
        Type viewModelMapper = null,
        bool makeRegistrationTypeGeneric = false) => WithControllerHelper(
        typeof(AbstractEntityReadController<,,,>),
        typeof(ICreateViewModelMapper<,,,>),
        registrationType,
        ReadViewModelType,
        viewModelMapper,
        makeRegistrationTypeGeneric);

    /// <summary>
    /// Registers a GET controller for the entity using a registration type
    /// </summary>
    /// <typeparam name="TRegistrationType">The service type to use</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithReadController<>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithReadController<TRegistrationType>()
        => WithReadController(typeof(TRegistrationType));

    /// <summary>
    /// Registers a GET controller for the entity using auto-generated types
    /// </summary>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithReadController()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithReadController()
        => WithReadController(
            typeof(AbstractEntityReadController<,,,>),
            makeRegistrationTypeGeneric: true);

    /// <summary>
    /// Registers a GET controller with search enabled for the entity
    /// </summary>
    /// <param name="registrationType"></param>
    /// <param name="viewModelMapper"></param>
    /// <param name="makeRegistrationTypeGeneric"></param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithSearchController()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithSearchController(Type registrationType,
        Type viewModelMapper = null,
        bool makeRegistrationTypeGeneric = false)
    {
        var mapperInterface = typeof(IReadViewModelMapper<,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), ReadViewModelType);

        if (makeRegistrationTypeGeneric)
        {
            registrationType = registrationType
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), Builder.SearchRequestType, SearchViewModelType, ReadViewModelType);
        }

        return WithControllerHelper(SearchControllerType(),
            mapperInterface,
            registrationType,
            ReadViewModelType,
            viewModelMapper,
            false,
            false);
    }

    /// <summary>
    /// Registers a GET controller with search enabled for the entity using a registration type
    /// </summary>
    /// <typeparam name="TRegistrationType">The service type to use</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithSearchController<>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithSearchController<TRegistrationType>()
        => WithSearchController(typeof(TRegistrationType));

    /// <summary>
    /// Registers a GET controller with search enabled for the entity using auto-generated types
    /// </summary>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithSearchController())
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithSearchController()
        => WithSearchController(typeof(AbstractEntitySearchController<,,,,,>),
            makeRegistrationTypeGeneric: true);

    /// <summary>
    /// Registers an UPDATE controller for the entity
    /// </summary>
    /// <param name="serviceType"></param>
    /// <param name="updateControllerType"></param>
    /// <param name="viewModelMapper"></param>
    /// <param name="resultModelTypeMapper"></param>
    /// <param name="makeServiceTypeGeneric"></param>
    /// <param name="registrationType"></param>
    /// <param name="viewModelType"></param>
    /// <param name="makeRegistrationTypeGeneric"></param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithUpdateController()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithUpdateController(Type serviceType,
        Func<Type> updateControllerType,
        Type viewModelMapper = null,
        Type resultModelTypeMapper = null,
        bool makeServiceTypeGeneric = false)
    {
        if (viewModelMapper != null)
        {
            var updateMapperInterface = typeof(IUpdateViewModelMapper<,,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityKeyType, typeof(TVersion), UpdateViewModelType);

            Builder.WithRegistration(updateMapperInterface, viewModelMapper, updateMapperInterface);
        }

        if (resultModelTypeMapper != null)
        {
            var resultMapperInterface = typeof(IReadViewModelMapper<,,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityKeyType, typeof(TVersion), ReadViewModelType);

            Builder.WithRegistration(resultMapperInterface, resultModelTypeMapper, resultMapperInterface);
        }

        if (makeServiceTypeGeneric)
        {
            serviceType = serviceType.MakeGenericType(Builder.EntityKeyType,
                Builder.EntityType,
                typeof(TVersion),
                UpdateViewModelType,
                UpdateViewModelBodyType,
                ReadViewModelType);
        }

        WithController(serviceType, updateControllerType());

        return this;
    }

    /// <summary>
    /// Registers an UPDATE controller for the entity using a registration type
    /// </summary>
    /// <typeparam name="TRegistrationType">The service type to use</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithUpdateController<>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithUpdateController<TRegistrationType>()
        => WithUpdateController(typeof(TRegistrationType), UpdateControllerType);

    /// <summary>
    /// Registers an UPDATE controller for the entity using auto-generated types
    /// </summary>
    /// <param name="includeValidationController">Optional;  Whether to include the Validate controller</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithUpdateController()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithUpdateController(bool includeValidationController = true)
    {
        if (includeValidationController)
        {
            WithValidateUpdateController();
        }
        return WithUpdateController(
            typeof(AbstractEntityUpdateController<,,,,,>),
            UpdateControllerType,
            makeServiceTypeGeneric: true);
    }

    /// <summary>
    /// Registers a Validate UPDATE controller for the entity using a registration type
    /// </summary>
    /// <typeparam name="TRegistrationType">The service type to use</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithValidateUpdateController<>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithValidateUpdateController<TRegistrationType>()
        => WithUpdateController(typeof(TRegistrationType), ValidateUpdateControllerType);

    /// <summary>
    /// Registers a Validate UPDATE controller for the entity using auto-generated types
    /// </summary>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithValidateUpdateController()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithValidateUpdateController()
        => WithUpdateController(
            typeof(AbstractEntityValidateUpdateController<,,,,,>),
            ValidateUpdateControllerType,
            makeServiceTypeGeneric: true);

    /// <summary>
    /// Registers a POST `/multiple` controller for the entity
    /// </summary>
    /// <param name="registrationType"></param>
    /// <param name="viewModelMapper"></param>
    /// <param name="resultModelTypeMapper"></param>
    /// <param name="makeRegistrationTypeGeneric"></param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithCreateMultipleController()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithCreateMultipleController(Type registrationType,
        Type viewModelMapper = null,
        Type resultModelTypeMapper = null,
        bool makeRegistrationTypeGeneric = false)
    {
        if (viewModelMapper != null)
        {
            var createMultipleViewModelMapperInterface = typeof(ICreateMultipleViewModelMapper<,,,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), CreateMultipleViewModelWrapperType, CreateMultipleViewModelType);

            Builder.WithRegistration(createMultipleViewModelMapperInterface, viewModelMapper,
                createMultipleViewModelMapperInterface);
        }

        if (resultModelTypeMapper != null)
        {
            var resultMapperInterface = typeof(IReadViewModelMapper<,,,>)
                .MakeGenericType(Builder.EntityKeyType, Builder.EntityKeyType, typeof(TVersion), ReadViewModelType);

            Builder.WithRegistration(resultMapperInterface, resultModelTypeMapper, resultMapperInterface);
        }

        if (makeRegistrationTypeGeneric)
        {
            registrationType = registrationType.MakeGenericType(Builder.EntityKeyType,
                Builder.EntityType,
                typeof(TVersion),
                CreateMultipleViewModelWrapperType,
                CreateMultipleViewModelType,
                ReadViewModelType);
        }

        WithController(registrationType, CreateMultipleControllerType());

        return this;
    }

    /// <summary>
    /// Registers a POST `/multiple` controller for the entity using a registration type
    /// </summary>
    /// <typeparam name="TRegistrationType">The service type to use</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithCreateMultipleController<>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithCreateMultipleController<TRegistrationType>()
        => WithCreateMultipleController(typeof(TRegistrationType));

    /// <summary>
    /// Registers a POST `/multiple` controller for the entity using auto-generated types
    /// </summary>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithCreateMultipleController()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithCreateMultipleController()
        => WithCreateMultipleController(typeof(AbstractEntityCreateMultipleController<,,,,,>),
            makeRegistrationTypeGeneric: true);

    /// <summary>
    /// Registers Create, Read, Update, Delete (and, optionally, Create Multiple and Get All) controllers for an entity using auto-generated types
    /// </summary>
    /// <param name="includeGetAll">Optional; Whether to include the Get All controller</param>
    /// <param name="includeMultipleCreate">Optional; Whether to include the Create Multiple controller</param>
    /// <param name="includeValidationController">Optional;  Whether to include the Validate controllers</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers(true, true)
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithAllControllers(bool includeGetAll = false,
        bool includeMultipleCreate = true, bool includeValidationController = true)
    {
        WithReadController();
        WithCreateController(includeValidationController);
        WithDeleteController();
        WithSearchController();
        WithUpdateController(includeValidationController);

        if (includeGetAll)
        {
            WithGetAllController();
        }

        if (includeMultipleCreate)
        {
            WithCreateMultipleController();
        }

        return this;
    }

    /// <summary>
    /// Registers a validation service for an entity
    /// </summary>
    /// <typeparam name="TService">The validation service to use</typeparam>
    /// <param name="replace">Whether to replace the existing validation service; default=<code>true</code></param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithValidationService<ValidationService>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithValidationService<TService>(bool replace = true)
    {
        var type = typeof(IEntityValidationService<,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion));

        if (!type.IsAssignableFrom(typeof(TService)))
        {
            throw new ArgumentException($"The service type {typeof(TService).Name} does not implement {type.Name}");
        }

        Builder.WithRegistration(type, typeof(TService), replace);

        return this;
    }

    /// <summary>
    /// Specify the max page size to use for Read endpoints (except Read `/all`)
    /// </summary>
    /// <param name="size">Optional, default = 100; The max page size to use</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers(true, true)
    ///          .WithMaxPageSize<ViewModel, ViewModelMapper>())
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithMaxPageSize(int size = 100)
    {
        Builder.WithRegistrationInstance<IMaxPageSize<TKey, TEntity, TVersion>>(new DefaultMaxPageSize<TEntity, TKey, TVersion>(size));

        return this;
    }
}
