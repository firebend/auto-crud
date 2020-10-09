using System;
using System.Collections.Generic;

namespace Firebend.AutoCrud.Core.Abstractions
{
    public abstract class BaseBuilder
    {
        public IDictionary<Type, Type> Registrations { get; set; }

        public virtual void Build()
        {
            
        }
    }
}