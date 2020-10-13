using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Web.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web
{
    public class ControllerEntityBuilder
    {
        private EntityCrudBuilder CrudBuilder { get; }
        
        public Dictionary<Type, List<string>> Policies { get;  } = new Dictionary<Type, List<string>>();
        
        public string Route { get; private set; }
        
        public string OpenApiGroupName { get; private set; }

        public ControllerEntityBuilder(EntityCrudBuilder builder)
        {
            CrudBuilder = builder;

            var name = string.IsNullOrWhiteSpace(CrudBuilder.EntityName)
                ? CrudBuilder.EntityType.Name
                : CrudBuilder.EntityName;

            WithRoute($"api/v1/{name.ToKebabCase()}");
            WithOpenApiGroupName(name.ToSentenceCase());
        }

        private ControllerEntityBuilder WithController(Type type, Type typeToCheck, params Type[] genericArgs)
        {
            var registrationType = type.MakeGenericType(genericArgs);
            var typeToCheckGeneric = typeToCheck.MakeGenericType(genericArgs);

            if (!typeToCheckGeneric.IsAssignableFrom(registrationType))
            {
                throw new Exception($"Registration type {registrationType} is not assignable to {typeToCheckGeneric}");
            }
            
            CrudBuilder.WithRegistration(registrationType, registrationType);
            
            return this;
        }

        public ControllerEntityBuilder WithRoute(string route)
        {
            Route = route;
            return this;
        }

        public ControllerEntityBuilder WithOpenApiGroupName(string openApiGroupName)
        {
            OpenApiGroupName = openApiGroupName;
            return this;
        }

        public ControllerEntityBuilder WithCreate(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntityCreateController<,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType);
        }
        
        public ControllerEntityBuilder WithCreate<TRegistrationType>()
        {
            return WithCreate(typeof(TRegistrationType));
        }
        
        public ControllerEntityBuilder WithCreate()
        {
            return WithCreate(typeof(AbstractEntityCreateController<,>));
        }
        
        public ControllerEntityBuilder WithDelete(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntityDeleteController<,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType);
        }
        
        public ControllerEntityBuilder WithDelete<TRegistrationType>()
        {
            return WithDelete(typeof(TRegistrationType));
        }
        
        public ControllerEntityBuilder WithDelete()
        {
            return WithDelete(typeof(AbstractEntityDeleteController<,>));
        }
        
        public ControllerEntityBuilder WithGetAll(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntityReadAllController<,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType);
        }
        
        public ControllerEntityBuilder WithGetAll<TRegistrationType>()
        {
            return WithGetAll(typeof(TRegistrationType));
        }
        
        public ControllerEntityBuilder WithGetAll()
        {
            return WithGetAll(typeof(AbstractEntityReadAllController<,>));
        }
        
        public ControllerEntityBuilder WithRead(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntityReadController<,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType);
        }
        
        public ControllerEntityBuilder WithRead<TRegistrationType>()
        {
            return WithGetAll(typeof(TRegistrationType));
        }
        
        public ControllerEntityBuilder WithRead()
        {
            return WithGetAll(typeof(AbstractEntityReadController<,>));
        }
        
        public ControllerEntityBuilder WithSearch(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntitySearchController<,,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CrudBuilder.SearchType);
        }
        
        public ControllerEntityBuilder WithSearch<TRegistrationType>()
        {
            return WithSearch(typeof(TRegistrationType));
        }
        
        public ControllerEntityBuilder WithSearch()
        {
            return WithSearch(typeof(AbstractEntitySearchController<,,>));
        }
        
        public ControllerEntityBuilder WithUpdate(Type registrationType)
        {
            return WithController(registrationType,
                typeof(AbstractEntityUpdateController<,>),
                CrudBuilder.EntityKeyType, CrudBuilder.EntityType);
        }
        
        public ControllerEntityBuilder WithUpdate<TRegistrationType>()
        {
            return WithUpdate(typeof(TRegistrationType));
        }
        
        public ControllerEntityBuilder WithUpdate()
        {
            return WithUpdate(typeof(AbstractEntityUpdateController<,>));
        }

        public ControllerEntityBuilder WithAll(bool includeGetAll = false)
        {
            WithCreate();
            WithDelete();
            WithRead();
            WithSearch();

            if (includeGetAll)
            {
                WithGetAll();
            }

            return this;
        }

        public ControllerEntityBuilder AddAuthorizationPolicy(Type type, string policy = "")
        {
            var attribute = new AuthorizeAttribute();

            if (!string.IsNullOrWhiteSpace(policy))
            {
                attribute.Policy = policy;
            }

            CrudBuilder.WithAttribute(type, attribute);

            return this;
        }

        public ControllerEntityBuilder AddAuthorizationPolicy<TController>(string policy)
        {
            return AddAuthorizationPolicy(typeof(TController), policy);
        }

        public ControllerEntityBuilder AddCreateAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntityCreateController<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType), policy);
        }

        public ControllerEntityBuilder AddDeleteAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntityDeleteController<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType), policy);
        }

        public ControllerEntityBuilder AddReadAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntityReadController<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType), policy);
        }

        public ControllerEntityBuilder AddReadAllAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntityReadAllController<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType), policy);
        }

        public ControllerEntityBuilder AddSearchAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntitySearchController<,,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType, CrudBuilder.SearchType), policy);
        }

        public ControllerEntityBuilder AddUpdateAuthorizationPolicy(string policy)
        {
            return AddAuthorizationPolicy(typeof(AbstractEntityUpdateController<,>).MakeGenericType(CrudBuilder.EntityKeyType, CrudBuilder.EntityType), policy);
        }
        public ControllerEntityBuilder AddAlterAuthorizationPolicies(string policy = "")
        {
            AddCreateAuthorizationPolicy(policy);
            AddDeleteAuthorizationPolicy(policy);
            AddSearchAuthorizationPolicy(policy);
            AddUpdateAuthorizationPolicy(policy);

            return this;
        }

        public ControllerEntityBuilder AddAuthorizationPolicies(string policy = "")
        {
            AddAlterAuthorizationPolicies(policy);
            AddReadAllAuthorizationPolicy(policy);
            AddReadAuthorizationPolicy(policy);

            return this;
        }
    }
}