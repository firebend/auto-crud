using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions
{
    public static class EntityBuilderExtensions
    {
        public static TBuilder WithEntity<TBuilder, TKey, TEntity>(this TBuilder builder, string entityName)
            where TBuilder : EntityBuilder<TKey, TEntity>
            where TKey : struct
            where TEntity : IEntity<TKey>
        {
            builder.EntityName = entityName;
            
            return builder;
        }

        
    }
}