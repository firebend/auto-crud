using System;

namespace Firebend.AutoCrud.Core.Abstractions
{
    public abstract class EntityBuilder : BaseBuilder
    {
        /// <summary>
        ///     Gets a value indicating the <see cref="Type" /> of Entity.
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        ///     Gets value indicating the <see cref="Type" /> key for the entity.
        /// </summary>
        public Type EntityKeyType { get; set; }

        public string EntityName { get; set; }
    }
}