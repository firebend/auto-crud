using System;
using System.Collections.Generic;
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