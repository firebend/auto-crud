using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Firebend.AutoCrud.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Abstractions.Builders
{
    public abstract class BaseBuilder
    {
        private readonly object _lock = new object();

        public bool IsBuilt { get; private set; }

        public IDictionary<Type, Registration> Registrations { get; set; }

        public Dictionary<Type, List<CrudBuilderAttributeModel>> Attributes { get; set; }
        
        public List<Action<IServiceCollection>> ServiceCollectionHooks { get; set; }
        
        public virtual string SignatureBase { get; }
        
        public void Build()
        {
            if (IsBuilt) return;

            lock (_lock)
            {
                if (IsBuilt) return;

                OnBuild();
                IsBuilt = true;
            }
        }

        protected virtual void OnBuild()
        {
        }
        
        public BaseBuilder WithRegistration(Type type, Registration registration, bool replace = true)
        {
            if (Registrations == null)
            {
                Registrations = new Dictionary<Type, Registration>
                {
                    { type, registration }
                };

                return this;
            }

            if (Registrations.ContainsKey(type))
            {
                if (replace)
                {
                    Registrations[type] = registration;
                }

                return this;
            }
            
            Registrations.Add(type, registration);

            return this;
        }

        public BaseBuilder WithRegistration(Type registrationType,
            Type serviceType,
            bool replace = true)
        {
            var registration = new ServiceRegistration
            {
                ServiceType = serviceType
            };

            return WithRegistration(registrationType, registration, replace);
        }

        public BaseBuilder WithRegistration<TRegistration, TService>(bool replace = true)
        {
            return WithRegistration(typeof(TRegistration), typeof(TService), replace);
        }

        public BaseBuilder WithRegistration<TRegistration>(Type type, bool replace = true)
        {
            return WithRegistration(typeof(TRegistration), type, replace);
        }

        public BaseBuilder WithRegistration(Type registrationType,
            Type serviceType,
            Type typeToCheck,
            bool replace = true)
        {
            if (!typeToCheck.IsAssignableFrom(serviceType))
            {
                throw new ArgumentException($"Registration type is not assignable to {typeToCheck}");
            }

            if (!typeToCheck.IsAssignableFrom(registrationType))
            {
                throw new ArgumentException($"Service type is not assignable to {typeToCheck}");
            }

            return WithRegistration(registrationType, serviceType, replace);
        }

        public BaseBuilder WithRegistrationInstance(Type registrationType, object instance)
        {
            var registration = new InstanceRegistration
            {
                Instance = instance,
                Lifetime = ServiceLifetime.Singleton
            };

            return WithRegistration(registrationType, registration);
        }

        public BaseBuilder WithRegistrationInstance<TInstance>(TInstance instance)
        {
            return WithRegistrationInstance(typeof(TInstance), instance);
        }

        public BaseBuilder WithDynamicClass(Type type, DynamicClassRegistration classRegistration)
        {
            WithRegistration(type, classRegistration);
            
            return this;
        }

        public BaseBuilder WithAttribute(Type registrationType, Type attributeType, CustomAttributeBuilder attribute)
        {
            Attributes ??= new Dictionary<Type, List<CrudBuilderAttributeModel>>();

            var model = new CrudBuilderAttributeModel
            {
                AttributeBuilder = attribute,
                AttributeType = attributeType
            };

            if (Attributes.ContainsKey(registrationType))
            {
                Attributes[registrationType] ??= new List<CrudBuilderAttributeModel>();
                Attributes[registrationType].RemoveAll(x => x.AttributeType == attributeType);
                Attributes[registrationType].Add(model);
            }
            else
            {
                Attributes.Add(registrationType, new List<CrudBuilderAttributeModel>
                {
                    model
                });
            }

            return this;
        }

        public BaseBuilder WithServiceCollectionHook(Action<IServiceCollection> action)
        {
            ServiceCollectionHooks??=new List<Action<IServiceCollection>>();
            ServiceCollectionHooks.Add(action);
            return this;
        }
    }
}