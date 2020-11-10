using System;
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Sockets;

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> : EntityCrudConfigurator<TBuilder, TKey, TEntity>
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TViewModel : class
    {
        public (Type attributeType, CustomAttributeBuilder attributeBuilder) DefaultAuthorizationPolicy { get; private set; }

        public bool HasDefaultAuthorizationPolicy => DefaultAuthorizationPolicy != default
                                                     && DefaultAuthorizationPolicy.attributeBuilder != null
                                                     && DefaultAuthorizationPolicy.attributeType != null;

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

            CrudBuilder.WithRegistration<IEntityValidationService<TKey, TEntity>, DefaultEntityValidationService<TKey, TEntity>>(false);
            CrudBuilder.WithRegistration<IEntityKeyParser<TKey, TEntity>, DefaultEntityKeyParser<TKey, TEntity>>(false);
            CrudBuilder.WithRegistration<IViewModelMapper<TKey, TEntity, TViewModel>, DefaultViewModelMapper<TKey, TEntity>>();
        }

        private TBuilder CrudBuilder { get; }

        public string Route { get; private set; }

        public string OpenApiGroupName { get; private set; }

        public string OpenApiEntityName { get; private set; }

        public string OpenApiEntityNamePlural { get; private set; }

        private (Type attributeType, CustomAttributeBuilder attributeBuilder) GetRouteAttributeInfo()
        {
            var routeType = typeof(RouteAttribute);
            var routeCtor = routeType.GetConstructor(new[] {typeof(string)});

            if (routeCtor == null)
            {
                return default;
            }

            var attributeBuilder = new CustomAttributeBuilder(routeCtor, new object[] {Route});

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
            var attributeCtor = attributeType.GetConstructor(new[] {typeof(string)});

            if (attributeCtor == null)
            {
                return default;
            }

            var attributeBuilder = new CustomAttributeBuilder(attributeCtor, new object[] {OpenApiGroupName});

            return (attributeType, attributeBuilder);
        }

        private (Type attributeType, CustomAttributeBuilder attributeBuilder) GetOpenApiEntityNameAttribute()
        {
            var attributeType = typeof(OpenApiEntityNameAttribute);

            var attributeCtor = attributeType.GetConstructor(new[]
            {
                typeof(string),
                typeof(string)
            });

            if (attributeCtor == null)
            {
                return default;
            }

            var attributeBuilder = new CustomAttributeBuilder(attributeCtor, new object[]
            {
                OpenApiEntityName,
                OpenApiEntityNamePlural
            });

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

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithController(Type type, Type typeToCheck, params Type[] genericArgs)
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

        public ControllerConfigurator<TBuilder, TKey, TEntity, TNewViewModel> WithViewModel<TNewViewModel, TViewModelMapper>()
            where TNewViewModel : class
            where TViewModelMapper : IViewModelMapper<TKey, TEntity, TNewViewModel>
        {
            if (GetRegisteredControllers().Any())
            {
                throw new Exception("Controllers have already been added. Please call .WithViewModel first in your configuration callback");
            }

            var config = new ControllerConfigurator<TBuilder, TKey, TEntity, TNewViewModel>(Builder);
            config.Builder.WithRegistration<IViewModelMapper<TKey, TEntity, TNewViewModel>, TViewModelMapper>();
            return config;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity, TNewViewModel> WithViewModel<TNewViewModel>(
                Func<TEntity, TNewViewModel> to,
                Func<TNewViewModel, TEntity> from)
            where TNewViewModel : class
        {
            if (GetRegisteredControllers().Any())
            {
                throw new Exception("Controllers have already been added. Please call .WithViewModel first in your configuration callback");
            }

            var config =  new ControllerConfigurator<TBuilder, TKey, TEntity, TNewViewModel>(Builder);

            var instance = new FunctionViewModelMapper<TKey, TEntity, TNewViewModel>
            {
                From = from,
                To = to
            };

            config.Builder.WithRegistrationInstance<IViewModelMapper<TKey, TEntity, TNewViewModel>>(instance);

            return config;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithController<TTypeCheck>(Type type)
            => WithController(type, typeof(TTypeCheck));

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithController<TController>()
            => WithController(typeof(TController), typeof(TController));

        private void AddAttributeToAllControllers(Type attributeType, CustomAttributeBuilder attributeBuilder) => GetRegisteredControllers()
            .ToList()
            .ForEach(x => { CrudBuilder.WithAttribute(x.Key, attributeType, attributeBuilder); });

        private IEnumerable<KeyValuePair<Type, Registration>> GetRegisteredControllers() => CrudBuilder
            .Registrations
            .SelectMany(x => x.Value, (pair, registration) => new KeyValuePair<Type, Registration>(pair.Key, registration))
            .Where(x => x.Value is ServiceRegistration)
            .Where(x => typeof(ControllerBase).IsAssignableFrom((x.Value as ServiceRegistration)?.ServiceType));

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithRoute(string route)
        {
            Route = route;
            var (aType, aBuilder) = GetRouteAttributeInfo();
            AddAttributeToAllControllers(aType, aBuilder);
            return this;
        }

        private void AddSwaggenGenOptionConfiguration()
        {
            Run.Once($"{GetType().FullName}.SwaggerGenOptions",() =>
            {
                Builder.WithServiceCollectionHook(sc =>
                {
                    sc.TryAddEnumerable(ServiceDescriptor.Transient<IPostConfigureOptions<SwaggerGenOptions>, PostConfigureSwaggerOptions>());
                });
            });
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithOpenApiGroupName(string openApiGroupName)
        {
            OpenApiGroupName = openApiGroupName;

            var (aType, aBuilder) = GetOpenApiGroupAttributeInfo();

            AddAttributeToAllControllers(aType, aBuilder);

            AddSwaggenGenOptionConfiguration();

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithOpenApiEntityName(string name, string plural = null)
        {
            OpenApiEntityName = name;
            OpenApiEntityNamePlural = plural ?? name.Pluralize();

            var (aType, aBuilder) = GetOpenApiEntityNameAttribute();

            AddAttributeToAllControllers(aType, aBuilder);

            AddSwaggenGenOptionConfiguration();

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithCreateController(Type registrationType)
            => WithController<AbstractEntityCreateController<TKey, TEntity, TViewModel>>(registrationType);

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithCreateController<TRegistrationType>()
            => WithCreateController(typeof(TRegistrationType));

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithCreateController()
            => WithCreateController<AbstractEntityCreateController<TKey, TEntity, TViewModel>>();

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithDeleteController(Type registrationType)
            => WithController<AbstractEntityDeleteController<TKey, TEntity, TViewModel>>(registrationType);

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithDeleteController<TRegistrationType>()
            => WithDeleteController(typeof(TRegistrationType));

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithDeleteController()
            => WithDeleteController<AbstractEntityDeleteController<TKey,TEntity, TViewModel>>();

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithGetAllController(Type registrationType)
            => WithController<AbstractEntityReadAllController<TKey, TEntity, TViewModel>>(registrationType);

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithGetAllController<TRegistrationType>() =>
            WithGetAllController(typeof(TRegistrationType));

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithGetAllController()
            => WithGetAllController<AbstractEntityReadAllController<TKey,TEntity, TViewModel>>();

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithReadController(Type registrationType)
            => WithController<AbstractEntityReadController<TKey,TEntity, TViewModel>>(registrationType);

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithReadController<TRegistrationType>()
            => WithReadController(typeof(TRegistrationType));

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithReadController()
            => WithReadController<AbstractEntityReadController<TKey, TEntity, TViewModel>>();

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithSearchController(Type registrationType)
            => WithController(registrationType,
            typeof(AbstractEntitySearchController<,,,>),
            CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CrudBuilder.SearchRequestType, typeof(TViewModel));

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithSearchController<TRegistrationType>()
            => WithSearchController(typeof(TRegistrationType));

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithSearchController()
            => WithSearchController(typeof(AbstractEntitySearchController<,,,>));

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithUpdateController(Type registrationType)
            => WithController<AbstractEntityUpdateController<TKey,TEntity, TViewModel>>(registrationType);

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithUpdateController<TRegistrationType>()
            => WithUpdateController(typeof(TRegistrationType));

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithUpdateController()
            => WithUpdateController<AbstractEntityUpdateController<TKey,TEntity, TViewModel>>();

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithAllControllers(bool includeGetAll = false)
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

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> AddAuthorizationPolicy(Type type, string authorizePolicy = "")
        {
            var (attributeType, attributeBuilder) = GetAuthorizationAttributeInfo(authorizePolicy);
            CrudBuilder.WithAttribute(type, attributeType, attributeBuilder);
            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> AddAuthorizationPolicy<TController>(string policy)
            => AddAuthorizationPolicy(typeof(TController), policy);

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> AddCreateAuthorizationPolicy(string policy)
            => AddAuthorizationPolicy<AbstractEntityCreateController<TKey,TEntity,TViewModel>>(policy);

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> AddDeleteAuthorizationPolicy(string policy)
            => AddAuthorizationPolicy<AbstractEntityDeleteController<TKey,TEntity,TViewModel>>(policy);

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> AddReadAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy<AbstractEntityReadController<TKey,TEntity,TViewModel>>(policy);
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> AddReadAllAuthorizationPolicy(string policy)
            => AddAuthorizationPolicy<AbstractEntityReadAllController<TKey,TEntity,TViewModel>>(policy);

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> AddSearchAuthorizationPolicy(string policy)
        {
            var type = typeof(AbstractEntitySearchController<,,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CrudBuilder.SearchType, typeof(TViewModel));

            return AddAuthorizationPolicy(type, policy);
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> AddUpdateAuthorizationPolicy(string policy)
            => AddAuthorizationPolicy<AbstractEntityUpdateController<TKey,TEntity,TViewModel>>(policy);

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> AddAlterAuthorizationPolicies(string policy = "")
        {
            AddCreateAuthorizationPolicy(policy);
            AddDeleteAuthorizationPolicy(policy);
            AddUpdateAuthorizationPolicy(policy);

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> AddQueryAuthorizationPolicies(string policy = "")
        {
            AddReadAuthorizationPolicy(policy);
            AddReadAllAuthorizationPolicy(policy);
            AddSearchAuthorizationPolicy(policy);

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> AddAuthorizationPolicies(string policy = "")
        {
            DefaultAuthorizationPolicy = GetAuthorizationAttributeInfo(policy);

            AddAttributeToAllControllers(DefaultAuthorizationPolicy.attributeType, DefaultAuthorizationPolicy.attributeBuilder);

            return this;
        }
    }
}
