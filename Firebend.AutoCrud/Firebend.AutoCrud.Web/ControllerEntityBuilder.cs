using System;
using System.Linq;
using System.Reflection.Emit;
using Firebend.AutoCrud.Core.Abstractions;
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
    public class ControllerEntityBuilder<TBuilder>
        where TBuilder : EntityCrudBuilder
    {
        public ControllerEntityBuilder(TBuilder builder)
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

        private ControllerEntityBuilder<TBuilder> WithController(Type type, Type typeToCheck, params Type[] genericArgs)
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

        public ControllerEntityBuilder<TBuilder> WithRoute(string route)
        {
            Route = route;
            var (aType, aBuilder) = GetRouteAttributeInfo();
            AddAttributeToAllControllers(aType, aBuilder);
            return this;
        }

        public ControllerEntityBuilder<TBuilder> WithOpenApiGroupName(string openApiGroupName)
        {
            OpenApiGroupName = openApiGroupName;
            var (aType, aBuilder) = GetOpenApiGroupAttributeInfo();
            AddAttributeToAllControllers(aType, aBuilder);
            return this;
        }

        public ControllerEntityBuilder<TBuilder> WithCreateController(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntityCreateController<,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType);
        }

        public ControllerEntityBuilder<TBuilder> WithCreateController<TRegistrationType>()
        {
            return WithCreateController(typeof(TRegistrationType));
        }

        public ControllerEntityBuilder<TBuilder> WithCreateController()
        {
            return WithCreateController(typeof(AbstractEntityCreateController<,>));
        }

        public ControllerEntityBuilder<TBuilder> WithDeleteController(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntityDeleteController<,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType);
        }

        public ControllerEntityBuilder<TBuilder> WithDeleteController<TRegistrationType>()
        {
            return WithDeleteController(typeof(TRegistrationType));
        }

        public ControllerEntityBuilder<TBuilder> WithDeleteController()
        {
            return WithDeleteController(typeof(AbstractEntityDeleteController<,>));
        }

        public ControllerEntityBuilder<TBuilder> WithGetAllController(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntityReadAllController<,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType);
        }

        public ControllerEntityBuilder<TBuilder> WithGetAllController<TRegistrationType>()
        {
            return WithGetAllController(typeof(TRegistrationType));
        }

        public ControllerEntityBuilder<TBuilder> WithGetAllController()
        {
            return WithGetAllController(typeof(AbstractEntityReadAllController<,>));
        }

        public ControllerEntityBuilder<TBuilder> WithReadController(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntityReadController<,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType);
        }

        public ControllerEntityBuilder<TBuilder> WithReadController<TRegistrationType>()
        {
            return WithReadController(typeof(TRegistrationType));
        }

        public ControllerEntityBuilder<TBuilder> WithReadController()
        {
            return WithReadController(typeof(AbstractEntityReadController<,>));
        }

        public ControllerEntityBuilder<TBuilder> WithSearchController(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntitySearchController<,,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CrudBuilder.SearchRequestType);
        }

        public ControllerEntityBuilder<TBuilder> WithSearchController<TRegistrationType>()
        {
            return WithSearchController(typeof(TRegistrationType));
        }

        public ControllerEntityBuilder<TBuilder> WithSearchController()
        {
            return WithSearchController(typeof(AbstractEntitySearchController<,,>));
        }

        public ControllerEntityBuilder<TBuilder> WithUpdateController(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntityUpdateController<,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType);
        }

        public ControllerEntityBuilder<TBuilder> WithUpdateController<TRegistrationType>()
        {
            return WithUpdateController(typeof(TRegistrationType));
        }

        public ControllerEntityBuilder<TBuilder> WithUpdateController()
        {
            return WithUpdateController(typeof(AbstractEntityUpdateController<,>));
        }

        public ControllerEntityBuilder<TBuilder> WithAllControllers(bool includeGetAll = false)
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

        public ControllerEntityBuilder<TBuilder> AddAuthorizationPolicy(Type type, string authorizePolicy = "")
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

        public ControllerEntityBuilder<TBuilder> AddAuthorizationPolicy<TController>(string policy)
        {
            return AddAuthorizationPolicy(typeof(TController), policy);
        }

        public ControllerEntityBuilder<TBuilder> AddCreateAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntityCreateController<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType), policy);
        }

        public ControllerEntityBuilder<TBuilder> AddDeleteAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntityDeleteController<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType), policy);
        }

        public ControllerEntityBuilder<TBuilder> AddReadAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntityReadController<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType), policy);
        }

        public ControllerEntityBuilder<TBuilder> AddReadAllAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntityReadAllController<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType),
                policy);
        }

        public ControllerEntityBuilder<TBuilder> AddSearchAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(
                typeof(AbstractEntitySearchController<,,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CrudBuilder.SearchType), policy);
        }

        public ControllerEntityBuilder<TBuilder> AddUpdateAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntityUpdateController<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType), policy);
        }

        public ControllerEntityBuilder<TBuilder> AddAlterAuthorizationPolicies(string policy = "")
        {
            AddCreateAuthorizationPolicy(policy);
            AddDeleteAuthorizationPolicy(policy);
            AddUpdateAuthorizationPolicy(policy);

            return this;
        }

        public ControllerEntityBuilder<TBuilder> AddAuthorizationPolicies(string policy = "")
        {
            AddAlterAuthorizationPolicies(policy);
            AddReadAllAuthorizationPolicy(policy);
            AddReadAuthorizationPolicy(policy);

            return this;
        }

        public TBuilder AsEntityBuilder()
        {
            return CrudBuilder;
        }
    }
}