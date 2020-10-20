using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions
{
    public static class EntityBuilderConfiguratorExtensions
    {
        public static EntityCrudBuilder<TKey, TEntity> AddCrud<TKey, TEntity>(this EntityCrudBuilder<TKey, TEntity> builder,
            Action<EntityCrudConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>> configure)
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            var config = new EntityCrudConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>(builder);
            configure(config);
            return builder;
        }
        
        public static TBuilder AddCrud<TBuilder, TKey, TEntity>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder<TKey, TEntity>
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            var config = new EntityCrudConfigurator<TBuilder, TKey, TEntity>(builder);
            config.WithCrud();
            return builder;
        }

        public static TBuilder AddDomainEvents<TBuilder, TKey, TEntity>(this TBuilder builder,
            Action<DomainEventsConfigurator<TBuilder, TKey, TEntity>> configure)
            where TBuilder : EntityCrudBuilder<TKey, TEntity>
            where TKey : struct
            where TEntity : IEntity<TKey>
        {
            var config = new DomainEventsConfigurator<TBuilder, TKey, TEntity>(builder);
            configure(config);
            return builder;
        }
    }
}