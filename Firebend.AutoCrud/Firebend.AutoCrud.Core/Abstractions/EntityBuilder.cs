using System;
using System.Collections.Generic;

namespace Firebend.AutoCrud.Core.Abstractions
{
    public abstract class EntityBuilder : BaseBuilder
    {
        /// <summary>
        /// Gets a value indicating the <see cref="Type"/> of Entity.
        /// </summary>
        public Type EntityType { get;  set; }
        
        /// <summary>
        /// Gets value indicating the <see cref="Type"/> key for the entity.
        /// </summary>
        public Type EntityKeyType { get;  set; }
        
        /// <summary>
        /// Gets a value indicating a friendly name for the entity used in routes.
        /// </summary>
        public string EntityName { get;  set; }
    }
}