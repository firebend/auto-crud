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
    public class ControllerConfigurator<TBuilder, TKey, TEntity> : EntityCrudConfigurator<TBuilder, TKey, TEntity>
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        public (Type attributeType, CustomAttributeBuilder attributeBuilder) DefaultAuthorizationPolicy { get; private set; }

        public bool HasDefaultAuthorizationPolicy => DefaultAuthorizationPolicy != default
                                                     && DefaultAuthorizationPolicy.attributeBuilder != null
                                                     && DefaultAuthorizationPolicy.attributeType != null;
        
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

            var name = (string.IsNullOrWhiteSpace(CrudBuilder.EntityName)
                ? CrudBuilder.EntityType.Name
                : CrudBuilder.EntityName).Humanize(LetterCasing.Title);

            OpenApiEntityName = name;
            OpenApiEntityNamePlural = name.Pluralize();

            WithRoute($"/api/v1/{name.Kebaberize()}");
            WithOpenApiGroupName(name);

            CrudBuilder.WithRegistration<IEntityValidationService<TKey, TEntity>, DefaultEntityValidationService<TKey, TEntity>>(false);
            CrudBuilder.WithRegistration<IEntityKeyParser<TKey, TEntity>, DefaultEntityKeyParser<TKey, TEntity>>(false);
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

            if (authCtor == null) return default;
            
            var args = !string.IsNullOrWhiteSpace(authorizePolicy)
                ? new object[] {authorizePolicy}
                : new object[] { };

            return (authType, new CustomAttributeBuilder(authCtor, args));

        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithController(Type type, Type typeToCheck, params Type[] genericArgs)
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
        {
            return WithController(type, typeof(TTypeCheck));
        }

        private void AddAttributeToAllControllers(Type attributeType, CustomAttributeBuilder attributeBuilder)
        {
            CrudBuilder
                .Registrations
                .SelectMany(x => x.Value, (pair, registration) => new
                {
                    pair.Key,
                    registration
                })
                .Where(x => x.registration is ServiceRegistration)
                .Where(x => typeof(ControllerBase).IsAssignableFrom((x.registration as ServiceRegistration)?.ServiceType))
                .ToList()
                .ForEach(x => { CrudBuilder.WithAttribute(x.Key, attributeType, attributeBuilder); });
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithRoute(string route)
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

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithOpenApiGroupName(string openApiGroupName)
        {
            OpenApiGroupName = openApiGroupName;
            
            var (aType, aBuilder) = GetOpenApiGroupAttributeInfo();
            
            AddAttributeToAllControllers(aType, aBuilder);
            
            AddSwaggenGenOptionConfiguration();

            return this;
        }
        
        public ControllerConfigurator<TBuilder, TKey, TEntity> WithOpenApiEntityName(string name, string plural = null)
        {
            OpenApiEntityName = name;
            OpenApiEntityNamePlural = plural ?? name.Pluralize();
            
            var (aType, aBuilder) = GetOpenApiEntityNameAttribute();
            
            AddAttributeToAllControllers(aType, aBuilder);
            
            AddSwaggenGenOptionConfiguration();

            return this;
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateController(Type registrationType)
        {
            return WithController<AbstractEntityCreateController<TKey, TEntity>>(registrationType);
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateController<TRegistrationType>()
        {
            return WithCreateController(typeof(TRegistrationType));
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateController()
        {
            return WithCreateController<AbstractEntityCreateController<TKey, TEntity>>();
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithDeleteController(Type registrationType)
        {
            return WithController<AbstractEntityDeleteController<TKey, TEntity>>(registrationType);
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithDeleteController<TRegistrationType>()
        {
            return WithDeleteController(typeof(TRegistrationType));
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithDeleteController()
        {
            return WithDeleteController<AbstractEntityDeleteController<TKey,TEntity>>();
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithGetAllController(Type registrationType)
        {
            return WithController<AbstractEntityReadAllController<TKey, TEntity>>(registrationType);
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithGetAllController<TRegistrationType>()
        {
            return WithGetAllController(typeof(TRegistrationType));
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithGetAllController()
        {
            return WithGetAllController<AbstractEntityReadAllController<TKey,TEntity>>();
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadController(Type registrationType)
        {
            return WithController<AbstractEntityReadController<TKey,TEntity>>(registrationType);
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadController<TRegistrationType>()
        {
            return WithReadController(typeof(TRegistrationType));
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadController()
        {
            return WithReadController<AbstractEntityReadController<TKey, TEntity>>();
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithSearchController(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntitySearchController<,,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CrudBuilder.SearchRequestType);
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithSearchController<TRegistrationType>()
        {
            return WithSearchController(typeof(TRegistrationType));
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithSearchController()
        {
            return WithSearchController(typeof(AbstractEntitySearchController<,,>));
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateController(Type registrationType)
        {
            return WithController<AbstractEntityUpdateController<TKey,TEntity>>(registrationType);
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateController<TRegistrationType>()
        {
            return WithUpdateController(typeof(TRegistrationType));
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateController()
        {
            return WithUpdateController<AbstractEntityUpdateController<TKey,TEntity>>();
        }

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
        {
            return AddAuthorizationPolicy(typeof(TController), policy);
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddCreateAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy<AbstractEntityCreateController<TKey,TEntity>>(policy);
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddDeleteAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy<AbstractEntityDeleteController<TKey,TEntity>>(policy);
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddReadAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy<AbstractEntityReadController<TKey,TEntity>>(policy);
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddReadAllAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy<AbstractEntityReadAllController<TKey,TEntity>>(policy);
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddSearchAuthorizationPolicy(string policy)
        {
            var type = typeof(AbstractEntitySearchController<,,>)
                .MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CrudBuilder.SearchType);
            
            return AddAuthorizationPolicy(type, policy);
        }

        public ControllerConfigurator<TBuilder, TKey, TEntity> AddUpdateAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy<AbstractEntityUpdateController<TKey,TEntity>>(policy);
        }

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
    }
}