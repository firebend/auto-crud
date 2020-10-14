using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Firebend.AutoCrud.Core.Abstractions
{
    public abstract class BaseBuilder
    {
        private readonly object _lock = new object();

        public bool IsBuilt { get; private set; }

        public IDictionary<Type, Type> Registrations { get; set; }

        public IDictionary<Type, object> InstanceRegistrations { get; set; }

        public IDictionary<Type, List<CustomAttributeBuilder>> Attributes { get; set; }

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