using System;
using System.Linq;
using System.Reflection.Emit;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Attributes;
using Firebend.AutoCrud.Web.Implementations;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web
{
    public class ControllerConfigurator<TBuilder> : EntityCrudConfigurator<TBuilder>
        where TBuilder : EntityCrudBuilder
    {
        public ControllerConfigurator(TBuilder builder) : base(builder)
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

            var name = string.IsNullOrWhiteSpace(CrudBuilder.EntityName)
                ? CrudBuilder.EntityType.Name
                : CrudBuilder.EntityName;

            WithRoute($"/api/v1/{name.ToKebabCase()}");
            WithOpenApiGroupName(name.ToSentenceCase());

            CrudBuilder.WithRegistration(
                typeof(IEntityValidationService<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType),
                typeof(DefaultEntityValidationService<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType),
                typeof(IEntityValidationService<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType));

            CrudBuilder.WithRegistration(
                typeof(IEntityKeyParser<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType),
                typeof(DefaultEntityKeyParser<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType),
                typeof(IEntityKeyParser<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType));
        }

        private TBuilder CrudBuilder { get; }

        public string Route { get; private set; }

        public string OpenApiGroupName { get; private set; }

        private (Type attributeType, CustomAttributeBuilder attributeBuilder) GetRouteAttributeInfo()
        {
            var routeType = typeof(RouteAttribute);
            var routeCtor = routeType.GetConstructor(new[] {typeof(string)});

            if (routeCtor == null) return default;

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

            if (attributeCtor == null) return default;

            var attributeBuilder = new CustomAttributeBuilder(attributeCtor, new object[] {OpenApiGroupName});

            return (attributeType, attributeBuilder);
        }

        private void AddOpenApiGroupNameAttribute(Type controllerType)
        {
            var (attributeType, attributeBuilder) = GetOpenApiGroupAttributeInfo();
            CrudBuilder.WithAttribute(controllerType, attributeType, attributeBuilder);
        }

        private ControllerConfigurator<TBuilder> WithController(Type type, Type typeToCheck, params Type[] genericArgs)
        {
            var registrationType = type.MakeGenericType(genericArgs);
            var typeToCheckGeneric = typeToCheck.MakeGenericType(genericArgs);

            if (!typeToCheckGeneric.IsAssignableFrom(registrationType))
            {
                throw new Exception($"Registration type {registrationType} is not assignable to {typeToCheckGeneric}");
            }

            CrudBuilder.WithRegistration(registrationType, registrationType);

            AddRouteAttribute(registrationType);
            AddOpenApiGroupNameAttribute(registrationType);

            return this;
        }

        private void AddAttributeToAllControllers(Type attributeType, CustomAttributeBuilder attributeBuilder)
        {
            CrudBuilder
                .Registrations
                .Where(x => x.Value is ServiceRegistration)
                .Where(x => typeof(ControllerBase).IsAssignableFrom((x.Value as ServiceRegistration)?.ServiceType))
                .ToList()
                .ForEach(x => { CrudBuilder.WithAttribute(x.Key, attributeType, attributeBuilder); });
        }

        public ControllerConfigurator<TBuilder> WithRoute(string route)
        {
            Route = route;
            var (aType, aBuilder) = GetRouteAttributeInfo();
            AddAttributeToAllControllers(aType, aBuilder);
            return this;
        }

        public ControllerConfigurator<TBuilder> WithOpenApiGroupName(string openApiGroupName)
        {
            OpenApiGroupName = openApiGroupName;
            var (aType, aBuilder) = GetOpenApiGroupAttributeInfo();
            AddAttributeToAllControllers(aType, aBuilder);
            return this;
        }

        public ControllerConfigurator<TBuilder> WithCreateController(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntityCreateController<,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType);
        }

        public ControllerConfigurator<TBuilder> WithCreateController<TRegistrationType>()
        {
            return WithCreateController(typeof(TRegistrationType));
        }

        public ControllerConfigurator<TBuilder> WithCreateController()
        {
            return WithCreateController(typeof(AbstractEntityCreateController<,>));
        }

        public ControllerConfigurator<TBuilder> WithDeleteController(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntityDeleteController<,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType);
        }

        public ControllerConfigurator<TBuilder> WithDeleteController<TRegistrationType>()
        {
            return WithDeleteController(typeof(TRegistrationType));
        }

        public ControllerConfigurator<TBuilder> WithDeleteController()
        {
            return WithDeleteController(typeof(AbstractEntityDeleteController<,>));
        }

        public ControllerConfigurator<TBuilder> WithGetAllController(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntityReadAllController<,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType);
        }

        public ControllerConfigurator<TBuilder> WithGetAllController<TRegistrationType>()
        {
            return WithGetAllController(typeof(TRegistrationType));
        }

        public ControllerConfigurator<TBuilder> WithGetAllController()
        {
            return WithGetAllController(typeof(AbstractEntityReadAllController<,>));
        }

        public ControllerConfigurator<TBuilder> WithReadController(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntityReadController<,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType);
        }

        public ControllerConfigurator<TBuilder> WithReadController<TRegistrationType>()
        {
            return WithReadController(typeof(TRegistrationType));
        }

        public ControllerConfigurator<TBuilder> WithReadController()
        {
            return WithReadController(typeof(AbstractEntityReadController<,>));
        }

        public ControllerConfigurator<TBuilder> WithSearchController(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntitySearchController<,,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CrudBuilder.SearchRequestType);
        }

        public ControllerConfigurator<TBuilder> WithSearchController<TRegistrationType>()
        {
            return WithSearchController(typeof(TRegistrationType));
        }

        public ControllerConfigurator<TBuilder> WithSearchController()
        {
            return WithSearchController(typeof(AbstractEntitySearchController<,,>));
        }

        public ControllerConfigurator<TBuilder> WithUpdateController(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntityUpdateController<,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType);
        }

        public ControllerConfigurator<TBuilder> WithUpdateController<TRegistrationType>()
        {
            return WithUpdateController(typeof(TRegistrationType));
        }

        public ControllerConfigurator<TBuilder> WithUpdateController()
        {
            return WithUpdateController(typeof(AbstractEntityUpdateController<,>));
        }

        public ControllerConfigurator<TBuilder> WithAllControllers(bool includeGetAll = false)
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

        public ControllerConfigurator<TBuilder> AddAuthorizationPolicy(Type type, string authorizePolicy = "")
        {
            var authType = typeof(AuthorizeAttribute);

            var authCtor = authorizePolicy == null
                ? null
                : authType.GetConstructor(!string.IsNullOrWhiteSpace(authorizePolicy)
                    ? new[] {typeof(string)}
                    : Type.EmptyTypes);

            if (authCtor != null)
            {
                var args = !string.IsNullOrWhiteSpace(authorizePolicy)
                    ? new object[] {authorizePolicy}
                    : new object[] { };

                CrudBuilder.WithAttribute(type, authType, new CustomAttributeBuilder(authCtor, args));
            }

            return this;
        }

        public ControllerConfigurator<TBuilder> AddAuthorizationPolicy<TController>(string policy)
        {
            return AddAuthorizationPolicy(typeof(TController), policy);
        }

        public ControllerConfigurator<TBuilder> AddCreateAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntityCreateController<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType), policy);
        }

        public ControllerConfigurator<TBuilder> AddDeleteAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntityDeleteController<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType), policy);
        }

        public ControllerConfigurator<TBuilder> AddReadAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntityReadController<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType), policy);
        }

        public ControllerConfigurator<TBuilder> AddReadAllAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntityReadAllController<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType),
                policy);
        }

        public ControllerConfigurator<TBuilder> AddSearchAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(
                typeof(AbstractEntitySearchController<,,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CrudBuilder.SearchType), policy);
        }

        public ControllerConfigurator<TBuilder> AddUpdateAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntityUpdateController<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType), policy);
        }

        public ControllerConfigurator<TBuilder> AddAlterAuthorizationPolicies(string policy = "")
        {
            AddCreateAuthorizationPolicy(policy);
            AddDeleteAuthorizationPolicy(policy);
            AddUpdateAuthorizationPolicy(policy);

            return this;
        }

        public ControllerConfigurator<TBuilder> AddAuthorizationPolicies(string policy = "")
        {
            AddAlterAuthorizationPolicies(policy);
            AddReadAllAuthorizationPolicy(policy);
            AddReadAuthorizationPolicy(policy);

            return this;
        }
    }
}