using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Models;

namespace Firebend.AutoCrud.Core.Abstractions
{
    public abstract class BaseBuilder
    {
        private readonly object _lock = new object();

        public bool IsBuilt { get; private set; }

        public IDictionary<Type, Type> Registrations { get; set; }

        public IDictionary<Type, object> InstanceRegistrations { get; set; }

        public IDictionary<Type, List<CrudBuilderAttributeModel>> Attributes { get; set; }
        
        public List<DynamicClassRegistration> DynamicClasses { get; set; }

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
    }
}