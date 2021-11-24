using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Abstractions.Builders
{
    public abstract class BaseBuilder
    {
        private readonly object _lock = new();

        public bool IsBuilt { get; private set; }

        public IDictionary<Type, List<Registration>> Registrations { get; set; }

        public Dictionary<Type, List<CrudBuilderAttributeModel>> Attributes { get; set; }

        public List<Action<IServiceCollection>> ServiceCollectionHooks { get; set; }

        public virtual string SignatureBase { get; set; }

        public void Build()
        {
            if (IsBuilt)
            {
                return;
            }

            lock (_lock)
            {
                if (IsBuilt)
                {
                    return;
                }

                OnBuild();
                IsBuilt = true;
            }
        }

        protected virtual void OnBuild()
        {
        }

        public BaseBuilder WithRegistration(Type type, Registration registration, bool replace = true, bool allowMany = false)
        {
            if (Registrations == null)
            {
                Registrations = new Dictionary<Type, List<Registration>> { { type, new List<Registration> { registration } } };

                return this;
            }

            if (Registrations.ContainsKey(type))
            {
                if (allowMany)
                {
                    Registrations[type] ??= new List<Registration>();
                    Registrations[type].Add(registration);
                }
                else if (replace)
                {
                    Registrations[type] = new List<Registration> { registration };
                }

                return this;
            }

            Registrations.Add(type, new List<Registration> { registration });

            return this;
        }

        public BaseBuilder WithRegistration(Type registrationType,
            Type serviceType,
            bool replace = true,
            bool allowMany = false,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            var registration = new ServiceRegistration { ServiceType = serviceType, Lifetime = lifetime};

            return WithRegistration(registrationType, registration, replace, allowMany);
        }

        public BaseBuilder WithRegistration<TRegistration, TService>(bool replace = true, bool allowMany = false, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            => WithRegistration(typeof(TRegistration), typeof(TService), replace, allowMany, serviceLifetime);

        public BaseBuilder WithRegistration<TRegistration>(Type type, bool replace = true, bool allowMany = false, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            => WithRegistration(typeof(TRegistration), type, replace, allowMany, serviceLifetime);

        public BaseBuilder WithRegistration(Type registrationType,
            Type serviceType,
            Type typeToCheck,
            bool replace = true,
            bool allowMany = false)
        {
            if (!typeToCheck.IsAssignableFrom(serviceType))
            {
                throw new ArgumentException($"Registration type is not assignable to {typeToCheck}");
            }

            if (!typeToCheck.IsAssignableFrom(registrationType))
            {
                throw new ArgumentException($"Service type is not assignable to {typeToCheck}");
            }

            return WithRegistration(registrationType, serviceType, replace, allowMany);
        }

        public BaseBuilder WithRegistrationInstance(Type registrationType, object instance, bool replace = true, bool allowMany = false)
        {
            var registration = new InstanceRegistration { Instance = instance, Lifetime = ServiceLifetime.Singleton };

            return WithRegistration(registrationType, registration, replace, allowMany);
        }

        public BaseBuilder WithRegistrationInstance<TInstance>(TInstance instance) => WithRegistrationInstance(typeof(TInstance), instance);

        public BaseBuilder WithRegistrationInstance<TInstance>(object instance) => WithRegistrationInstance(typeof(TInstance), instance);

        public BaseBuilder WithDynamicClass(Type type, DynamicClassRegistration classRegistration)
        {
            WithRegistration(type, classRegistration);

            return this;
        }

        public BaseBuilder WithAttribute(Type registrationType, Type attributeType, CustomAttributeBuilder attribute)
        {
            Attributes ??= new Dictionary<Type, List<CrudBuilderAttributeModel>>();

            var model = new CrudBuilderAttributeModel { AttributeBuilder = attribute, AttributeType = attributeType };

            if (Attributes.ContainsKey(registrationType))
            {
                Attributes[registrationType] ??= new List<CrudBuilderAttributeModel>();
                Attributes[registrationType].RemoveAll(x => x.AttributeType == attributeType);
                Attributes[registrationType].Add(model);
            }
            else
            {
                Attributes.Add(registrationType, new List<CrudBuilderAttributeModel> { model });
            }

            return this;
        }

        public BaseBuilder WithServiceCollectionHook(Action<IServiceCollection> action)
        {
            ServiceCollectionHooks ??= new List<Action<IServiceCollection>>();
            ServiceCollectionHooks.Add(action);
            return this;
        }

        public bool HasRegistration(Type type) => Registrations != null && Registrations.ContainsKey(type);

        public bool HasRegistration<TRegistration>() => HasRegistration(typeof(TRegistration));

        public void EnsureRegistered(Type type)
        {
            if (!HasRegistration(type))
            {
                string argsString = null;

                if (type.GenericTypeArguments.HasValues())
                {
                    var args = type
                        .GenericTypeArguments
                        .Aggregate(new StringBuilder(), (a, b) => a.Append(b.Name).Append(","));

                    argsString = args.ToString(0, args.Length - 1);

                }
                throw new Exception($"Please register a {type.Name} {argsString}");
            }
        }
        public void EnsureRegistered<TRegistration>() => EnsureRegistered(typeof(TRegistration));
    }
}
