// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.Core.Threading;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Attributes;
using Firebend.AutoCrud.Web.Implementations;
using Firebend.AutoCrud.Web.Implementations.Options;
using Firebend.AutoCrud.Web.Interfaces;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Firebend.AutoCrud.Web
{
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

            CrudBuilder.WithRegistration<ICreateViewModelMapper<TKey, TEntity, TEntity>, DefaultViewModelMapper<TKey, TEntity>>();
            CrudBuilder.WithRegistration<IUpdateViewModelMapper<TKey, TEntity, TEntity>, DefaultViewModelMapper<TKey, TEntity>>();
            CrudBuilder.WithRegistration<IReadViewModelMapper<TKey, TEntity, TEntity>, DefaultViewModelMapper<TKey, TEntity>>();

            CreateViewModelType = CrudBuilder.EntityType;
            ReadViewModelType = CrudBuilder.EntityType;
            UpdateViewModelType = CrudBuilder.EntityType;
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

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithController<TTypeCheck>(Type type)
            => WithController(type, typeof(TTypeCheck));

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

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithRoute(string route)
        {
            Route = route;
            var (aType, aBuilder) = GetRouteAttributeInfo();
            AddAttributeToAllControllers(aType, aBuilder);
            return this;
        }

        private void AddSwaggerGenOptionConfiguration() => Run.Once($"{GetType().FullName}.SwaggerGenOptions", () =>
        {
            Builder.WithServiceCollectionHook(sc =>
            {
                sc.TryAddEnumerable(ServiceDescriptor.Transient<IPostConfigureOptions<SwaggerGenOptions>, PostConfigureSwaggerOptions>());
            });
        });

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithOpenApiGroupName(string openApiGroupName)
        {
            OpenApiGroupName = openApiGroupName;

            var (aType, aBuilder) = GetOpenApiGroupAttributeInfo();

            AddAttributeToAllControllers(aType, aBuilder);

            AddSwaggerGenOptionConfiguration();

            return this;
        }

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

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateController<TRegistrationType>()
            => WithCreateController(typeof(TRegistrationType),
                CreateViewModelType,
                null,
                ReadViewModelType,
                null,
                false);

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateController()
            => WithCreateController(
                typeof(AbstractEntityCreateController<,,,>),
                CreateViewModelType,
                null,
                ReadViewModelType,
                null,
                true);

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

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithDeleteController<TRegistrationType>()
            => WithDeleteController(typeof(TRegistrationType), ReadViewModelType);

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithDeleteController()
            => WithDeleteController(typeof(AbstractEntityDeleteController<,,>),
                ReadViewModelType,
                null,
                true);

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

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithGetAllController<TRegistrationType>() =>
            WithGetAllController(typeof(TRegistrationType), CrudBuilder.EntityType);

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithGetAllController()
            => WithGetAllController(typeof(AbstractEntityReadAllController<,,>),
                ReadViewModelType,
                null,
                true);

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

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadController<TRegistrationType>()
            => WithReadController(typeof(TRegistrationType), ReadViewModelType);

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadController()
            => WithReadController(
                typeof(AbstractEntityReadController<,,>),
                ReadViewModelType,
                null,
                true);

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
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithSearchController<TRegistrationType>()
            => WithSearchController(typeof(TRegistrationType), ReadViewModelType);

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithSearchController()
            => WithSearchController(typeof(AbstractEntitySearchController<,,,>),
                ReadViewModelType,
                null,
                true);

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

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateController<TRegistrationType>()
            => WithUpdateController(typeof(TRegistrationType),
                UpdateViewModelType,
                null,
                ReadViewModelType,
                null,
                false);

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateController()
            => WithUpdateController(
                typeof(AbstractEntityUpdateController<,,,>),
                UpdateViewModelType,
                null,
                ReadViewModelType,
                null,
                true);

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithAllControllers(bool includeGetAll = false)
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

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddAuthorizationPolicy(Type type, string authorizePolicy = "")
        {
            var (attributeType, attributeBuilder) = GetAuthorizationAttributeInfo(authorizePolicy);
            CrudBuilder.WithAttribute(type, attributeType, attributeBuilder);
            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddAuthorizationPolicy<TController>(string policy)
            => AddAuthorizationPolicy(typeof(TController), policy);

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddCreateAuthorizationPolicy(string policy)
            => AddAuthorizationPolicy(typeof(AbstractEntityCreateController<,,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CreateViewModelType, ReadViewModelType), policy);

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddDeleteAuthorizationPolicy(string policy)
            => AddAuthorizationPolicy(typeof(AbstractEntityDeleteController<,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, ReadViewModelType), policy);

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddReadAuthorizationPolicy(string policy)
            => AddAuthorizationPolicy(typeof(AbstractEntityReadController<,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, ReadViewModelType), policy);

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddReadAllAuthorizationPolicy(string policy)
            => AddAuthorizationPolicy(typeof(AbstractEntityReadAllController<,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, ReadViewModelType), policy);

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddSearchAuthorizationPolicy(string policy)
        {
            var type = typeof(AbstractEntitySearchController<,,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CrudBuilder.SearchType, ReadViewModelType);

            return AddAuthorizationPolicy(type, policy);
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddUpdateAuthorizationPolicy(string policy)
            => AddAuthorizationPolicy(typeof(AbstractEntityUpdateController<,,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, ReadViewModelType, UpdateViewModelType), policy);

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddAlterAuthorizationPolicies(string policy = "")
        {
            AddCreateAuthorizationPolicy(policy);
            AddDeleteAuthorizationPolicy(policy);
            AddUpdateAuthorizationPolicy(policy);

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddQueryAuthorizationPolicies(string policy = "")
        {
            AddReadAuthorizationPolicy(policy);
            AddReadAllAuthorizationPolicy(policy);
            AddSearchAuthorizationPolicy(policy);

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddAuthorizationPolicies(string policy = "")
        {
            DefaultAuthorizationPolicy = GetAuthorizationAttributeInfo(policy);

            AddAttributeToAllControllers(DefaultAuthorizationPolicy.attributeType, DefaultAuthorizationPolicy.attributeBuilder);

            return this;
        }

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

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateViewModel(Type viewModelType, Type viewModelMapper)
        {
            ViewModelGuard("Please register a Create view model before adding controllers");

            CreateViewModelType = viewModelType;

            var mapper = typeof(ICreateViewModelMapper<,,>)
                .MakeGenericType(CrudBuilder.EntityType, CrudBuilder.EntityType, viewModelType);

            CrudBuilder.WithRegistration(mapper, viewModelMapper);

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateViewModel<TViewModel, TViewModelMapper>()
            where TViewModel : class
            where TViewModelMapper : ICreateViewModelMapper<TKey, TEntity, TViewModel>
        {
            ViewModelGuard("Please register a Create view model before adding controllers");

            CreateViewModelType = typeof(TViewModel);

            CrudBuilder.WithRegistration<ICreateViewModelMapper<TKey, TEntity, TViewModel>, TViewModelMapper>();

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateViewModel<TViewModel>(
            Func<TViewModel, TEntity> from)
            where TViewModel : class
        {
            ViewModelGuard("Please register read view model before adding controllers.");

            var instance = new FunctionViewModelMapper<TKey, TEntity, TViewModel>
            {
                From = from
            };

            CreateViewModelType = typeof(TViewModel);

            CrudBuilder.WithRegistrationInstance<ICreateViewModelMapper<TKey, TEntity, TViewModel>>(instance);

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadViewModel(Type viewModelType, Type viewModelMapper)
        {
            ViewModelGuard("Please register a read view model before adding controllers");

            ReadViewModelType = viewModelType;

            var mapper = typeof(IReadViewModelMapper<,,>)
                .MakeGenericType(CrudBuilder.EntityType, CrudBuilder.EntityType, viewModelType);

            CrudBuilder.WithRegistration(mapper, viewModelMapper);

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadViewModel<TViewModel, TViewModelMapper>()
            where TViewModel : class
            where TViewModelMapper : IReadViewModelMapper<TKey, TEntity, TViewModel>
        {
            ViewModelGuard("Please register a read view model before adding controllers");

            ReadViewModelType = typeof(TViewModel);

            CrudBuilder.WithRegistration<IReadViewModelMapper<TKey, TEntity, TViewModel>, TViewModelMapper>();

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadViewModel<TViewModel>(
            Func<TEntity, TViewModel> to)
            where TViewModel : class
        {
            ViewModelGuard("Please registered read view model before adding controllers");

            var instance = new FunctionViewModelMapper<TKey, TEntity, TViewModel>
            {
                To = to
            };

            ReadViewModelType = typeof(TViewModel);

            CrudBuilder.WithRegistrationInstance<IReadViewModelMapper<TKey, TEntity, TViewModel>>(instance);

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateViewModel(Type viewModelType, Type viewModelMapper)
        {
            ViewModelGuard("Please register a Update view model before adding controllers");

            UpdateViewModelType = viewModelType;

            var mapper = typeof(IUpdateViewModelMapper<,,>)
                .MakeGenericType(CrudBuilder.EntityType, CrudBuilder.EntityType, viewModelType);

            CrudBuilder.WithRegistration(mapper, viewModelMapper);

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateViewModel<TViewModel, TViewModelMapper>()
            where TViewModel : class
            where TViewModelMapper : IUpdateViewModelMapper<TKey, TEntity, TViewModel>
        {
            ViewModelGuard("Please register a update view model before adding controllers");

            UpdateViewModelType = typeof(TViewModel);

            CrudBuilder.WithRegistration<IUpdateViewModelMapper<TKey, TEntity, TViewModel>, TViewModelMapper>();

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateViewModel<TViewModel>(
            Func<TViewModel, TEntity> from)
            where TViewModel : class, IEntity<TKey>
        {
            ViewModelGuard("Please register a update view model before adding controllers");

            var instance = new FunctionViewModelMapper<TKey, TEntity, TViewModel>
            {
                From = from,
            };

            UpdateViewModelType = typeof(TViewModel);

            CrudBuilder.WithRegistrationInstance<IUpdateViewModelMapper<TKey, TEntity, TViewModel>>(instance);

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithViewModel(Type viewModelType, Type viewModelMapper)
        {
            ViewModelGuard("Please register a view model before adding controllers");

            WithCreateViewModel(viewModelType, viewModelMapper);
            WithUpdateViewModel(viewModelType, viewModelMapper);
            WithReadViewModel(viewModelType, viewModelMapper);

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithViewModel<TViewModel, TViewModelMapper>()
            where TViewModel : class, IEntity<TKey>
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

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithViewModel<TViewModel>(
                Func<TEntity, TViewModel> to,
                Func<TViewModel, TEntity> from)
            where TViewModel : class
        {
            ViewModelGuard("Please register view model before adding controllers");

            var instance = new FunctionViewModelMapper<TKey, TEntity, TViewModel>
            {
                From = from,
                To = to
            };

            CreateViewModelType = typeof(TViewModel);
            UpdateViewModelType = typeof(TViewModel);
            ReadViewModelType = typeof(TViewModel);

            CrudBuilder.WithRegistrationInstance<ICreateViewModelMapper<TKey, TEntity, TViewModel>>(instance);
            CrudBuilder.WithRegistrationInstance<IUpdateViewModelMapper<TKey, TEntity, TViewModel>>(instance);
            CrudBuilder.WithRegistrationInstance<IReadViewModelMapper<TKey, TEntity, TViewModel>>(instance);

            return this;
        }

    }
}
