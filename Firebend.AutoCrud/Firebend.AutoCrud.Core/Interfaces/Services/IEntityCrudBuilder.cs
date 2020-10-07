using System;

namespace Firebend.AutoCrud.Core.Interfaces.Services
{
    public interface IEntityCrudBuilder
    {
        /// <summary>
        /// Gets a value indicating the <see cref="Type"/> of Entity.
        /// </summary>
        public Type EntityType { get; internal set; }
        
        /// <summary>
        /// Gets value indicating the <see cref="Type"/> key for the entity.
        /// </summary>
        public Type EntityKeyType { get; internal set; }
        
        /// <summary>
        /// Gets a value indicating a friendly name for the entity used in routes.
        /// </summary>
        public string EntityName { get; internal set; }
        
        /// <summary>
        /// Gets a value indicating the entity route prefix. i.e api/v1/
        /// </summary>
        public string RoutePrefix { get; internal set; }
        
        /// <summary>
        /// Gets a value indicating whether or not a GET endpoint will be exposed that allows for all entities to be retrieved at once.
        /// </summary>
        public bool IncludeGetAllEndpoint { get; internal set; }
    }

    public static class EntityCrudBuilderExtensions
    {
        public static TBuilder ForEntity<TBuilder, TEntity, TEntityKey>(this TBuilder builder)
            where TBuilder : IEntityCrudBuilder
            where TEntity : IEntity<TEntityKey>
            where TEntityKey : struct
        {
            builder.EntityType = typeof(TEntity);
            builder.EntityKeyType = typeof(TEntityKey);

            return builder;
        }

        public static TBuilder WithEntityName<TBuilder, TEntity, TEntityKey>(this TBuilder builder, string entityName)
            where TBuilder : IEntityCrudBuilder
            where TEntity : IEntity<TEntityKey>
            where TEntityKey : struct
        {
            builder.EntityName = entityName;
            return builder;
        }

        public static TBuilder WithRoute<TBuilder, TEntity, TEntityKey>(this TBuilder builder, string route)
            where TBuilder : IEntityCrudBuilder
            where TEntity : IEntity<TEntityKey>
            where TEntityKey : struct
        {
            builder.RoutePrefix = route;
            return builder;
        }

        public static TBuilder WIthGetAllEndpoint<TBuilder, TEntity, TEntityKey>(this TBuilder builder, bool getAll)
            where TBuilder : IEntityCrudBuilder
            where TEntity : IEntity<TEntityKey>
            where TEntityKey : struct
        {
            builder.IncludeGetAllEndpoint = getAll;
            return builder;
        }
    }
}