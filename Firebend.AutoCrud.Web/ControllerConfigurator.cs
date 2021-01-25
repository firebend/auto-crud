// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.Core.Threading;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Attributes;
using Firebend.AutoCrud.Web.Implementations.Options;
using Firebend.AutoCrud.Web.Implementations.Paging;
using Firebend.AutoCrud.Web.Implementations.ViewModelMappers;
using Firebend.AutoCrud.Web.Interfaces;
using Firebend.AutoCrud.Web.Models;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Firebend.AutoCrud.Web
{
    public static class ControllerConfiguratorCache
    {
        // ReSharper disable once InconsistentNaming
        public static bool IsSwaggerApplied;
        public static readonly object Lock = new ();
    }

    public class ControllerConfigurator<TBuilder, TKey, TEntity> : EntityCrudConfigurator<TBuilder, TKey, TEntity>
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
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

            CrudBuilder = builder;

            var name = (string.IsNullOrWhiteSpace(CrudBuilder.EntityName)
                ? CrudBuilder.EntityType.Name
                : CrudBuilder.EntityName).Humanize(LetterCasing.Title);

            OpenApiEntityName = name;
            OpenApiEntityNamePlural = name.Pluralize();

            WithRoute($"/api/v1/{name.Kebaberize()}");
            WithOpenApiGroupName(name);

            WithValidationService<DefaultEntityValidationService<TKey, TEntity>>(false);

            CrudBuilder.WithRegistration<IEntityKeyParser<TKey, TEntity>, DefaultEntityKeyParser<TKey, TEntity>>(false);

            WithCreateViewModel<DefaultCreateUpdateViewModel<TKey, TEntity>, DefaultCreateViewModelMapper<TKey, TEntity>>();
            WithReadViewModel<TEntity, DefaultReadViewModelMapper<TKey, TEntity>>();
            WithUpdateViewModel<DefaultCreateUpdateViewModel<TKey, TEntity>, DefaultUpdateViewModelMapper<TKey, TEntity>>();
            WithCreateMultipleViewModel<MultipleEntityViewModel<TEntity>, TEntity, DefaultCreateMultipleViewModelMapper<TKey, TEntity>>();
            WithMaxPageSize();
        }

        public (Type attributeType, CustomAttributeBuilder attributeBuilder) DefaultAuthorizationPolicy { get; private set; }

        public bool HasDefaultAuthorizationPolicy => DefaultAuthorizationPolicy != default
                                                     && DefaultAuthorizationPolicy.attributeBuilder != null
                                                     && DefaultAuthorizationPolicy.attributeType != null;

        private TBuilder CrudBuilder { get; }

        public string Route { get; private set; }

        public string OpenApiGroupName { get; private set; }

        public string OpenApiEntityName { get; private set; }

        public string OpenApiEntityNamePlural { get; private set; }

        public Type CreateViewModelType { get; private set; }

        public Type UpdateViewModelType { get; private set; }

        public Type ReadViewModelType { get; private set; }

        public Type CreateMultipleViewModelWrapperType { get; private set; }

        public Type CreateMultipleViewModelType { get; private set; }

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

            CrudBuilder.WithAttribute(controllerType, routeType, attributeBuilder);
        }

        private (Type attributeType, CustomAttributeBuilder attributeBuilder) GetOpenApiGroupAttributeInfo()
        {
            var attributeType = typeof(OpenApiGroupNameAttribute);
            var attributeCtor = attributeType.GetConstructor(new[] { typeof(string) });

            if (attributeCtor == null)
            {
                return default;
            }

            var attributeBuilder = new CustomAttributeBuilder(attributeCtor, new object[] { OpenApiGroupName });

            return (attributeType, attributeBuilder);
        }

        private (Type attributeType, CustomAttributeBuilder attributeBuilder) GetOpenApiEntityNameAttribute()
        {
            var attributeType = typeof(OpenApiEntityNameAttribute);

            var attributeCtor = attributeType.GetConstructor(new[] { typeof(string), typeof(string) });

            if (attributeCtor == null)
            {
                return default;
            }

            var attributeBuilder = new CustomAttributeBuilder(attributeCtor, new object[] { OpenApiEntityName, OpenApiEntityNamePlural });

            return (attributeType, attributeBuilder);
        }

        private void AddOpenApiGroupNameAttribute(Type controllerType)
        {
            var (attributeType, attributeBuilder) = GetOpenApiGroupAttributeInfo();
            CrudBuilder.WithAttribute(controllerType, attributeType, attributeBuilder);
        }

        private void AddOpenApiEntityNameAttribute(Type controllerType)
        {
            var (attributeType, attributeBuilder) = GetOpenApiEntityNameAttribute();
            CrudBuilder.WithAttribute(controllerType, attributeType, attributeBuilder);
        }

        private static (Type attributeType, CustomAttributeBuilder attributeBuilder) GetAuthorizationAttributeInfo(string authorizePolicy = "")
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithController(Type type,
            Type typeToCheck,
            params Type[] genericArgs)
        {
            var hasGenerics = genericArgs != null && genericArgs.Length > 0;
            var registrationType = hasGenerics ? type.MakeGenericType(genericArgs) : type;

            var typeToCheckGeneric = hasGenerics ? typeToCheck.MakeGenericType(genericArgs) : typeToCheck;

            if (!typeToCheckGeneric.IsAssignableFrom(registrationType))
            {
                throw new Exception($"Registration type {registrationType} is not assignable to {typeToCheckGeneric}");
            }

            CrudBuilder.WithRegistration(registrationType, registrationType);

            AddRouteAttribute(registrationType);
            AddOpenApiGroupNameAttribute(registrationType);
            AddOpenApiEntityNameAttribute(registrationType);

            if (HasDefaultAuthorizationPolicy)
            {
                CrudBuilder.WithAttribute(registrationType, DefaultAuthorizationPolicy.attributeType, DefaultAuthorizationPolicy.attributeBuilder);
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithController<TTypeCheck>(Type type)
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithController<TController>()
            => WithController(typeof(TController), typeof(TController));

        private void AddAttributeToAllControllers(Type attributeType, CustomAttributeBuilder attributeBuilder) => GetRegisteredControllers()
            .ToList()
            .ForEach(x => { CrudBuilder.WithAttribute(x.Key, attributeType, attributeBuilder); });

        private IEnumerable<KeyValuePair<Type, Registration>> GetRegisteredControllers() => CrudBuilder
            .Registrations
            .SelectMany(x => x.Value, (pair, registration) => new KeyValuePair<Type, Registration>(pair.Key, registration))
            .Where(x => x.Value is ServiceRegistration)
            .Where(x => typeof(ControllerBase).IsAssignableFrom((x.Value as ServiceRegistration)?.ServiceType));

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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithRoute(string route)
        {
            Route = route;
            var (aType, aBuilder) = GetRouteAttributeInfo();
            AddAttributeToAllControllers(aType, aBuilder);
            return this;
        }

        private void AddSwaggerGenOptionConfiguration()
        {
            if (ControllerConfiguratorCache.IsSwaggerApplied)
            {
                return;
            }

            using var _ = new AsyncDuplicateLock().Lock(ControllerConfiguratorCache.Lock);
            {
                if (ControllerConfiguratorCache.IsSwaggerApplied)
                {
                    return;
                }

                Builder.WithServiceCollectionHook(sc =>
                {
                    sc.TryAddEnumerable(ServiceDescriptor.Transient<IPostConfigureOptions<SwaggerGenOptions>, PostConfigureSwaggerOptions>());
                });

                ControllerConfiguratorCache.IsSwaggerApplied = true;
            }
        }

        /// <summary>
        /// Specifies the group name to list an entity under for OpenApi and Swagger documentation
        /// </summary>
        /// <param name="openApiGroupName">The group name to use</param>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers(true)
        ///          .WithOpenApiGroupName("Weather Forecasts")
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithOpenApiGroupName(string openApiGroupName)
        {
            OpenApiGroupName = openApiGroupName;

            var (aType, aBuilder) = GetOpenApiGroupAttributeInfo();

            AddAttributeToAllControllers(aType, aBuilder);

            AddSwaggerGenOptionConfiguration();

            return this;
        }

        /// <summary>
        /// Specifies the entity name to use fo an entity under in OpenApi and Swagger documentation
        /// </summary>
        /// <param name="name">The entity name to use</param>
        /// <param name="plural">Optional: the entity name to use when a plural is required, automatically pluralized if not provided</param>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers(true)
        ///          .WithOpenApiGroupName("Weather Forecast")
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithOpenApiEntityName(string name, string plural = null)
        {
            OpenApiEntityName = name;
            OpenApiEntityNamePlural = plural ?? name.Pluralize();

            var (aType, aBuilder) = GetOpenApiEntityNameAttribute();

            AddAttributeToAllControllers(aType, aBuilder);

            AddSwaggerGenOptionConfiguration();

            return this;
        }

        private ControllerConfigurator<TBuilder, TKey, TEntity> WithControllerHelper(
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
                    .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, viewModelType);

                mapper = mapperInterfaceType
                    .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, viewModelType);
            }
            else
            {
                controller = controllerType;
                mapper = mapperInterfaceType;
            }

            if (makeRegistrationTypeGeneric)
            {
                registrationType = registrationType
                    .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, viewModelType);
            }

            WithController(controller, registrationType);

            if (viewModelMapper != null)
            {
                CrudBuilder.WithRegistration(mapper, viewModelMapper, mapper);
            }

            return this;
        }

        /// <summary>
        /// Registers a CREATE controller for the entity
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="viewModelType"></param>
        /// <param name="viewModelMapper"></param>
        /// <param name="resultModelType"></param>
        /// <param name="resultModelTypeMapper"></param>
        /// <param name="makeServiceGeneric"></param>
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateController(Type serviceType,
            Type viewModelType,
            Type viewModelMapper,
            Type resultModelType,
            Type resultModelTypeMapper,
            bool makeServiceGeneric)
        {
            var controller = typeof(AbstractEntityCreateController<,,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType,
                    CrudBuilder.EntityType,
                    viewModelType,
                    resultModelType);

            if (viewModelMapper != null)
            {
                var createViewModelMapperInterface = typeof(ICreateViewModelMapper<,,>)
                    .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityKeyType, viewModelType);

                CrudBuilder.WithRegistration(createViewModelMapperInterface, viewModelMapper, createViewModelMapperInterface);
            }

            if (resultModelTypeMapper != null)
            {
                var resultMapperInterface = typeof(IReadViewModelMapper<,,>)
                    .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityKeyType, resultModelType);

                CrudBuilder.WithRegistration(resultMapperInterface, resultModelTypeMapper, resultMapperInterface);
            }

            if (makeServiceGeneric)
            {
                serviceType = serviceType.MakeGenericType(CrudBuilder.EntityKeyType,
                        CrudBuilder.EntityType,
                        viewModelType,
                        resultModelType);
            }

            WithController(serviceType, controller);

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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateController<TRegistrationType>()
            => WithCreateController(typeof(TRegistrationType),
                CreateViewModelType,
                null,
                ReadViewModelType,
                null,
                false);

        /// <summary>
        /// Registers a CREATE controller for the entity using auto-generated types
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateController()
            => WithCreateController(
                typeof(AbstractEntityCreateController<,,,>),
                CreateViewModelType,
                null,
                ReadViewModelType,
                null,
                true);

        /// <summary>
        /// Registers a DELETE controller for the entity
        /// </summary>
        /// <param name="registrationType"></param>
        /// <param name="viewModelType"></param>
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithDeleteController(Type registrationType,
            Type viewModelType,
             Type viewModelMapper = null,
            bool makeRegistrationTypeGeneric = false) => WithControllerHelper(
            typeof(AbstractEntityDeleteController<,,>),
        typeof(ICreateViewModelMapper<,,>),
            registrationType,
            viewModelType,
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithDeleteController<TRegistrationType>()
            => WithDeleteController(typeof(TRegistrationType), ReadViewModelType);

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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithDeleteController()
            => WithDeleteController(typeof(AbstractEntityDeleteController<,,>),
                ReadViewModelType,
                null,
                true);

        /// <summary>
        /// Registers a GET `/all` controller for the entity
        /// </summary>
        /// <param name="registrationType"></param>
        /// <param name="viewModelType"></param>
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithGetAllController(Type registrationType,
            Type viewModelType,
            Type viewModelMapper = null,
            bool makeRegistrationTypeGeneric = false) => WithControllerHelper(
            typeof(AbstractEntityReadAllController<,,>),
        typeof(ICreateViewModelMapper<,,>),
            registrationType,
            viewModelType,
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithGetAllController<TRegistrationType>() =>
            WithGetAllController(typeof(TRegistrationType), CrudBuilder.EntityType);

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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithGetAllController()
            => WithGetAllController(typeof(AbstractEntityReadAllController<,,>),
                ReadViewModelType,
                null,
                true);

        /// <summary>
        /// Registers a GET controller for the entity
        /// </summary>
        /// <param name="registrationType"></param>
        /// <param name="viewModelType"></param>
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadController(Type registrationType,
            Type viewModelType,
            Type viewModelMapper = null,
            bool makeRegistrationTypeGeneric = false) => WithControllerHelper(
            typeof(AbstractEntityReadController<,,>),
            typeof(ICreateViewModelMapper<,,>),
            registrationType,
            viewModelType,
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadController<TRegistrationType>()
            => WithReadController(typeof(TRegistrationType), ReadViewModelType);

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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadController()
            => WithReadController(
                typeof(AbstractEntityReadController<,,>),
                ReadViewModelType,
                null,
                true);

        /// <summary>
        /// Registers a GET controller with search enabled for the entity
        /// </summary>
        /// <param name="registrationType"></param>
        /// <param name="viewModelType"></param>
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithSearchController(Type registrationType,
            Type viewModelType,
            Type viewModelMapper = null,
            bool makeRegistrationTypeGeneric = false)
        {
            var controller = typeof(AbstractEntitySearchController<,,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CrudBuilder.SearchRequestType, viewModelType);

            var mapperInterface = typeof(IReadViewModelMapper<,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, viewModelType);

            if (makeRegistrationTypeGeneric)
            {
                registrationType = registrationType
                    .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CrudBuilder.SearchRequestType, viewModelType);
            }

            return WithControllerHelper(controller,
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithSearchController<TRegistrationType>()
            => WithSearchController(typeof(TRegistrationType), ReadViewModelType);

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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithSearchController()
            => WithSearchController(typeof(AbstractEntitySearchController<,,,>),
                ReadViewModelType,
                null,
                true);

        /// <summary>
        /// Registers an UPDATE controller for the entity
        /// </summary>
        /// <param name="registrationType"></param>
        /// <param name="viewModelType"></param>
        /// <param name="viewModelMapper"></param>
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateController(Type serviceType,
            Type viewModelType,
            Type viewModelMapper,
            Type resultModelType,
            Type resultModelTypeMapper,
            bool makeServiceTypeGeneric)
        {
            var controller = typeof(AbstractEntityUpdateController<,,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType,
                    CrudBuilder.EntityType,
                    viewModelType,
                    resultModelType);

            if (viewModelMapper != null)
            {
                var updateMapperInterface = typeof(IUpdateViewModelMapper<,,>)
                    .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityKeyType, viewModelType);

                CrudBuilder.WithRegistration(updateMapperInterface, viewModelMapper, updateMapperInterface);
            }

            if (resultModelTypeMapper != null)
            {
                var resultMapperInterface = typeof(IReadViewModelMapper<,,>)
                    .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityKeyType, resultModelType);

                CrudBuilder.WithRegistration(resultMapperInterface, resultModelTypeMapper, resultMapperInterface);
            }

            if (makeServiceTypeGeneric)
            {
                serviceType = serviceType.MakeGenericType(CrudBuilder.EntityKeyType,
                        CrudBuilder.EntityType,
                        viewModelType,
                        resultModelType);
            }

            WithController(serviceType, controller);

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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateController<TRegistrationType>()
            => WithUpdateController(typeof(TRegistrationType),
                UpdateViewModelType,
                null,
                ReadViewModelType,
                null,
                false);

        /// <summary>
        /// Registers an UPDATE controller for the entity using auto-generated types
        /// </summary>
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateController()
            => WithUpdateController(
                typeof(AbstractEntityUpdateController<,,,>),
                UpdateViewModelType,
                null,
                ReadViewModelType,
                null,
                true);

        /// <summary>
        /// Registers a POST `/multiple` controller for the entity
        /// </summary>
        /// <param name="registrationType"></param>
        /// <param name="viewModelType"></param>
        /// <param name="viewModelMapper"></param>
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateMultipleController(Type serviceType,
            Type viewModelWrapperType,
            Type viewModelType,
            Type viewModelMapper,
            Type resultModelType,
            Type resultModelTypeMapper,
            bool makeServiceGeneric)
        {
            var controller = typeof(AbstractEntityCreateMultipleController<,,,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType,
                    CrudBuilder.EntityType,
                    viewModelWrapperType,
                    viewModelType,
                    resultModelType);

            if (viewModelMapper != null)
            {
                var createMultipleViewModelMapperInterface = typeof(ICreateMultipleViewModelMapper<,,,>)
                    .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, viewModelWrapperType, viewModelType);

                CrudBuilder.WithRegistration(createMultipleViewModelMapperInterface, viewModelMapper, createMultipleViewModelMapperInterface);
            }

            if (resultModelTypeMapper != null)
            {
                var resultMapperInterface = typeof(IReadViewModelMapper<,,>)
                    .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityKeyType, resultModelType);

                CrudBuilder.WithRegistration(resultMapperInterface, resultModelTypeMapper, resultMapperInterface);
            }

            if (makeServiceGeneric)
            {
                serviceType = serviceType.MakeGenericType(CrudBuilder.EntityKeyType,
                        CrudBuilder.EntityType,
                        viewModelWrapperType,
                        viewModelType,
                        resultModelType);
            }

            WithController(serviceType, controller);

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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateMultipleController<TRegistrationType>()
            => WithCreateMultipleController(typeof(TRegistrationType),
                CreateMultipleViewModelWrapperType,
                CreateMultipleViewModelType,
                null,
                ReadViewModelType,
                null,
                false);

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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateMultipleController()
            => WithCreateMultipleController(typeof(AbstractEntityCreateMultipleController<,,,,>),
                CreateMultipleViewModelWrapperType,
                CreateMultipleViewModelType,
                null,
                ReadViewModelType,
                null,
                true);

        /// <summary>
        /// Registers Create, Read, Update, Delete (and, optionally, Create Multiple and Get All) controllers for an entity using auto-generated types
        /// </summary>
        /// <param name="includeGetAll">Optional; Whether to include the Get All controller</param>
        /// <param name="includeMultipleCreate">Optional; Whether to include the Create Multiple controller</param>
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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithAllControllers(bool includeGetAll = false, bool includeMultipleCreate = true)
        {
            WithReadController();
            WithCreateController();
            WithDeleteController();
            WithSearchController();
            WithUpdateController();

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
        ///          .AddAuthorizationPlicy()
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> AddAuthorizationPolicy(Type type, string authorizePolicy = "")
        {
            var (attributeType, attributeBuilder) = GetAuthorizationAttributeInfo(authorizePolicy);
            CrudBuilder.WithAttribute(type, attributeType, attributeBuilder);
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
        ///          .AddAuthorizationPlicy<Controller>("Policy")
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
        ///          .AddCreateAuthorizationPlicy("Policy")
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> AddCreateAuthorizationPolicy(string policy)
            => AddAuthorizationPolicy(typeof(AbstractEntityCreateController<,,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CreateViewModelType, ReadViewModelType), policy);

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
        ///          .AddDeleteAuthorizationPlicy("Policy")
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> AddDeleteAuthorizationPolicy(string policy)
            => AddAuthorizationPolicy(typeof(AbstractEntityDeleteController<,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, ReadViewModelType), policy);

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
        ///          .AddReadAuthorizationPlicy("Policy")
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> AddReadAuthorizationPolicy(string policy)
            => AddAuthorizationPolicy(typeof(AbstractEntityReadController<,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, ReadViewModelType), policy);

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
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, ReadViewModelType), policy);

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
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CrudBuilder.SearchType, ReadViewModelType);

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
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, ReadViewModelType, UpdateViewModelType), policy);

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

            AddAttributeToAllControllers(DefaultAuthorizationPolicy.attributeType, DefaultAuthorizationPolicy.attributeBuilder);

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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithValidationService<TService>(bool replace = true)
            where TService : IEntityValidationService<TKey, TEntity>
        {
            CrudBuilder.WithRegistration<IEntityValidationService<TKey, TEntity>, TService>(replace);
            return this;
        }

        private void ViewModelGuard(string msg)
        {
            if (GetRegisteredControllers().Any())
            {
                throw new Exception($"Controllers are already registered. {msg}");
            }
        }

        /// <summary>
        /// Specify a custom view model to use for the entity Create endpoint
        /// </summary>
        /// <param name="viewModelType">The type of the view model to use</param>
        /// <param name="viewModelMapper">The type of the view model mapper to use</param>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers()
        ///          .WithCreateViewModel(typeof(ViewModel), typeof(ViewModelMapper))
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateViewModel(Type viewModelType, Type viewModelMapper)
        {
            ViewModelGuard("Please register a Create view model before adding controllers");

            CreateViewModelType = viewModelType;

            var mapper = typeof(ICreateViewModelMapper<,,>)
                .MakeGenericType(CrudBuilder.EntityType, CrudBuilder.EntityType, viewModelType);

            CrudBuilder.WithRegistration(mapper, viewModelMapper, mapper);

            return this;
        }

        /// <summary>
        /// Specify a custom view model to use for the entity Create endpoint
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model to use</typeparam>
        /// <typeparam name="TViewModelMapper">The type of the view model mapper to use</typeparam>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers()
        ///          .WithCreateViewModel<ViewModel, ViewModelWrapper>()
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateViewModel<TViewModel, TViewModelMapper>()
            where TViewModel : class
            where TViewModelMapper : ICreateViewModelMapper<TKey, TEntity, TViewModel>
        {
            ViewModelGuard("Please register a Create view model before adding controllers");

            CreateViewModelType = typeof(TViewModel);

            CrudBuilder.WithRegistration<ICreateViewModelMapper<TKey, TEntity, TViewModel>, TViewModelMapper>();

            return this;
        }

        /// <summary>
        /// Specify a custom view model to use for the entity Create endpoint
        /// </summary>
        /// <param name="from">A callback function that maps the view model to the entity class</typeparam>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers()
        ///          .WithCreateViewModel<ViewModel>(viewModel => {
        ///              var e = new WeatherForecast();
        ///              viewModel?.Body?.CopyPropertiesTo(e);
        ///              return e;
        ///          }))
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateViewModel<TViewModel>(
            Func<TViewModel, TEntity> from)
            where TViewModel : class
        {
            ViewModelGuard("Please register read view model before adding controllers.");

            var instance = new FunctionViewModelMapper<TKey, TEntity, TViewModel>(from);

            CreateViewModelType = typeof(TViewModel);

            CrudBuilder.WithRegistrationInstance<ICreateViewModelMapper<TKey, TEntity, TViewModel>>(instance);

            return this;
        }

        /// <summary>
        /// Specify a custom view model to use for the entity Read endpoint
        /// </summary>
        /// <param name="viewModelType">The type of the view model to use</param>
        /// <param name="viewModelMapper">The type of the view model mapper to use</param>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers()
        ///          .WithReadViewModel(typeof(ViewModel), typeof(ViewModelMapper))
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadViewModel(Type viewModelType, Type viewModelMapper)
        {
            ViewModelGuard("Please register a read view model before adding controllers");

            ReadViewModelType = viewModelType;

            var mapper = typeof(IReadViewModelMapper<,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, viewModelType);

            CrudBuilder.WithRegistration(mapper, viewModelMapper, mapper);

            return this;
        }

        /// <summary>
        /// Specify a custom view model to use for the entity Read endpoint
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model to use</typeparam>
        /// <typeparam name="TViewModelMapper">The type of the view model mapper to use</typeparam>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers()
        ///          .WithReadViewModel<ViewModel, ViewModelWrapper>()
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadViewModel<TViewModel, TViewModelMapper>()
            where TViewModel : class
            where TViewModelMapper : IReadViewModelMapper<TKey, TEntity, TViewModel>
        {
            ViewModelGuard("Please register a read view model before adding controllers");

            ReadViewModelType = typeof(TViewModel);

            CrudBuilder.WithRegistration<IReadViewModelMapper<TKey, TEntity, TViewModel>, TViewModelMapper>();

            return this;
        }

        /// <summary>
        /// Specify a custom view model to use for the entity Read endpoint
        /// </summary>
        /// <param name="to">A callback function that maps the entity to the view model class</param>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers()
        ///          .WithReadViewModel<ViewModel>(entity => new ViewModel(entity)))
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadViewModel<TViewModel>(
            Func<TEntity, TViewModel> to)
            where TViewModel : class
        {
            ViewModelGuard("Please registered read view model before adding controllers");

            var instance = new FunctionViewModelMapper<TKey, TEntity, TViewModel>(to);

            ReadViewModelType = typeof(TViewModel);

            CrudBuilder.WithRegistrationInstance<IReadViewModelMapper<TKey, TEntity, TViewModel>>(instance);

            return this;
        }

        /// <summary>
        /// Specify a custom view model to use for the entity Update endpoint
        /// </summary>
        /// <param name="viewModelType">The type of the view model to use</param>
        /// <param name="viewModelMapper">The type of the view model mapper to use</param>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers()
        ///          .WithUpdateViewModel(typeof(ViewModel), typeof(ViewModelMapper))
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateViewModel(Type viewModelType, Type viewModelMapper)
        {
            ViewModelGuard("Please register a Update view model before adding controllers");

            UpdateViewModelType = viewModelType;

            var mapper = typeof(IUpdateViewModelMapper<,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, viewModelType);

            CrudBuilder.WithRegistration(mapper, viewModelMapper, mapper);

            return this;
        }

        /// <summary>
        /// Specify a custom view model to use for the entity Update endpoint
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model to use</typeparam>
        /// <typeparam name="TViewModelMapper">The type of the view model mapper to use</typeparam>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers()
        ///          .WithUpdateViewModel<ViewModel, ViewModelWrapper>()
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateViewModel<TViewModel, TViewModelMapper>()
            where TViewModel : class
            where TViewModelMapper : IUpdateViewModelMapper<TKey, TEntity, TViewModel>
        {
            ViewModelGuard("Please register a update view model before adding controllers");

            UpdateViewModelType = typeof(TViewModel);

            CrudBuilder.WithRegistration<IUpdateViewModelMapper<TKey, TEntity, TViewModel>, TViewModelMapper>();

            return this;
        }

        /// <summary>
        /// Specify a custom view model to use for the entity Update endpoint
        /// </summary>
        /// <param name="from">A callback function that maps the view model to the entity class</typeparam>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers()
        ///          .WithUpdateViewModel<ViewModel>(viewModel => {
        ///              var e = new WeatherForecast();
        ///              viewModel?.Body?.CopyPropertiesTo(e);
        ///              return e;
        ///          }))
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateViewModel<TViewModel>(
            Func<TViewModel, TEntity> from)
            where TViewModel : class
        {
            ViewModelGuard("Please register a update view model before adding controllers");

            var instance = new FunctionViewModelMapper<TKey, TEntity, TViewModel>(from);

            UpdateViewModelType = typeof(TViewModel);

            CrudBuilder.WithRegistrationInstance<IUpdateViewModelMapper<TKey, TEntity, TViewModel>>(instance);

            return this;
        }

        /// <summary>
        /// Specify a custom view model to use for the entity Create `/multiple` endpoint
        /// </summary>
        /// <param name="viewWrapper">The type of the view model wrapper to use</param>
        /// <param name="view">The type of the view model to use</param>
        /// <param name="viewMapper">The type of the view model mapper to use</param>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers()
        ///          .WithCreateMultipleViewModel(type(ViewWrapper), typeof(ViewModel), typeof(ViewModelMapper))
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateMultipleViewModel(Type viewWrapper,
            Type view,
            Type viewMapper)
        {
            ViewModelGuard("Please register a Update view model before adding controllers");

            CreateMultipleViewModelType = view;
            CreateMultipleViewModelWrapperType = viewWrapper;

            var mapper = typeof(ICreateMultipleViewModelMapper<,,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, viewWrapper, view);

            CrudBuilder.WithRegistration(mapper, viewMapper, mapper);

            return this;
        }

        /// <summary>
        /// Specify a custom view model to use for the entity Create `/multiple` endpoint
        /// </summary>
        /// <typeparam name="TViewWrapper">The type of the view model wrapper to use</typeparam>
        /// <typeparam name="TView">The type of the view model to use</typeparam>
        /// <typeparam name="TMapper">The type of the view model mapper to use</typeparam>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers()
        ///          .WithCreateMultipleViewModel<ViewWrapper, ViewModel, ViewModelMapper>()
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateMultipleViewModel<TViewWrapper, TView, TMapper>()
            where TView : class
            where TViewWrapper : IMultipleEntityViewModel<TView>
            where TMapper : ICreateMultipleViewModelMapper<TKey, TEntity, TViewWrapper, TView>
        {
            CreateMultipleViewModelType = typeof(TView);
            CreateMultipleViewModelWrapperType = typeof(TViewWrapper);

            CrudBuilder.WithRegistration<ICreateMultipleViewModelMapper<TKey, TEntity, TViewWrapper, TView>, TMapper>();

            return this;
        }

        /// <summary>
        /// Specify a custom view model to use for the entity Create `/multiple` endpoint
        /// </summary>
        /// <typeparam name="TViewWrapper">The type of the view model wrapper to use</typeparam>
        /// <typeparam name="TView">The type of the view model to use</typeparam>
        /// <param name="mapperFunc">A callback function that maps a view model to the entity class</typeparam>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers()
        ///          .WithCreateMultipleViewModel<ViewWrapper, ViewModel>(viewModel => {
        ///              var e = new WeatherForecast();
        ///              viewModel?.Body?.CopyPropertiesTo(e);
        ///              return e;
        ///          }))
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateMultipleViewModel<TViewWrapper, TView>(
                Func<TViewWrapper, TView, TEntity> mapperFunc)
            where TView : class
            where TViewWrapper : IMultipleEntityViewModel<TView>
        {
            CreateMultipleViewModelType = typeof(TView);
            CreateMultipleViewModelWrapperType = typeof(TViewWrapper);

            var instance = new FunctionCreateMultipleViewModelMapper<TKey, TEntity, TViewWrapper, TView>
            {
                Func = mapperFunc
            };

            CrudBuilder.WithRegistrationInstance<ICreateMultipleViewModelMapper<TKey, TEntity, TViewWrapper, TView>>(instance);

            return this;
        }

        /// <summary>
        /// Specify a custom view model to use for the entity Create, Update, and Read endpoints
        /// </summary>
        /// <param name="viewModelType">The type of the view model to use</param>
        /// <param name="viewModelMapper">The type of the view model mapper to use</param>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers()
        ///          .WithViewModel(ypeof(ViewModel), typeof(ViewModelMapper))
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithViewModel(Type viewModelType, Type viewModelMapper)
        {
            ViewModelGuard("Please register a view model before adding controllers");

            WithCreateViewModel(viewModelType, viewModelMapper);
            WithUpdateViewModel(viewModelType, viewModelMapper);
            WithReadViewModel(viewModelType, viewModelMapper);

            return this;
        }

        /// <summary>
        /// Specify a custom view model to use for the entity Create, Update, and Read endpoints
        /// </summary>
        /// <param name="viewModelType">The type of the view model to use</param>
        /// <param name="viewModelMapper">The type of the view model mapper to use</param>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers()
        ///          .WithViewModel(ypeof(ViewModel), typeof(ViewModelMapper))
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithViewModel<TViewModel, TViewModelMapper>()
            where TViewModel : class
            where TViewModelMapper : IUpdateViewModelMapper<TKey, TEntity, TViewModel>,
                ICreateViewModelMapper<TKey, TEntity, TViewModel>,
                IReadViewModelMapper<TKey, TEntity, TViewModel>
        {
            ViewModelGuard("Please register a view model before adding controllers");

            WithCreateViewModel<TViewModel, TViewModelMapper>();
            WithUpdateViewModel<TViewModel, TViewModelMapper>();
            WithReadViewModel<TViewModel, TViewModelMapper>();

            return this;
        }

        /// <summary>
        /// Specify a custom view model to use for the entity Create, Update, and Read endpoints
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model to use</typeparam>
        /// <param name="to">A callback function that maps the entity to the view model class</param>
        /// <param name="from">A callback function that maps the view model to the entity class</param>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers()
        ///          .WithViewModel<ViewModel>(
        ///             entity => new ViewModel(entity)
        ///             viewModel => new WeatherForecast(viewModel)
        ///          ))
        /// </code>
        /// </example>
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithViewModel<TViewModel>(
                Func<TEntity, TViewModel> to,
                Func<TViewModel, TEntity> from)
            where TViewModel : class
        {
            ViewModelGuard("Please register view model before adding controllers");

            var instance = new FunctionViewModelMapper<TKey, TEntity, TViewModel>(from, to);

            CreateViewModelType = typeof(TViewModel);
            UpdateViewModelType = typeof(TViewModel);
            ReadViewModelType = typeof(TViewModel);

            CrudBuilder.WithRegistrationInstance<ICreateViewModelMapper<TKey, TEntity, TViewModel>>(instance);
            CrudBuilder.WithRegistrationInstance<IUpdateViewModelMapper<TKey, TEntity, TViewModel>>(instance);
            CrudBuilder.WithRegistrationInstance<IReadViewModelMapper<TKey, TEntity, TViewModel>>(instance);

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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithMaxPageSize(int size = 100)
        {
            CrudBuilder.WithRegistrationInstance<IMaxPageSize<TKey, TEntity>>(new DefaultMaxPageSize<TEntity, TKey>(size));

            return this;
        }
    }
}
