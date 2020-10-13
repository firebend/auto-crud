using System;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Web.Abstractions;

namespace Firebend.AutoCrud.Web
{
    public class ControllerEntityBuilder
    {
        public EntityCrudBuilder CrudBuilder { get; }

        public ControllerEntityBuilder(EntityCrudBuilder builder)
        {
            CrudBuilder = builder;
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
    }
}