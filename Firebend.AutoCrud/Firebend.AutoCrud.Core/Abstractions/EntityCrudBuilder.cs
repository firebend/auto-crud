using System;
using System.Collections.Generic;

namespace Firebend.AutoCrud.Core.Abstractions
{
    public abstract class EntityCrudBuilder
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
        
        /// <summary>
        /// Gets a value indicating the entity route prefix. i.e api/v1/
        /// </summary>
        public string RoutePrefix { get; set; }
        
        /// <summary>
        /// Gets a value indicating whether or not a GET endpoint will be exposed that allows for all entities to be retrieved at once.
        /// </summary>
        public bool IncludeGetAllEndpoint { get; set; }
        
        public IDictionary<Type, Type> Registrations { get; set; }
        
        public IDictionary<Type, string> ControllerPolicies { get; set; }
    }
}