using System;
using System.Collections.Generic;

namespace Firebend.AutoCrud.Core.Abstractions
{
    public abstract class BaseBuilder
    {
        private object _lock = new object();
        
        public bool IsBuilt { get; private set; }
        
        public IDictionary<Type, Type> Registrations { get; set; }
        
        public IDictionary<Type, object> InstanceRegistrations { get; set; }

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
    }
}