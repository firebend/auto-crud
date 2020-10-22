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
        
        public static EntityCrudBuilder<TKey, TEntity> AddCrud<TKey, TEntity>(this EntityCrudBuilder<TKey, TEntity> builder)
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            return AddCrud(builder, crud => crud.WithCrud());
        }

        public static EntityCrudBuilder<TKey, TEntity> AddDomainEvents<TKey, TEntity>(this EntityCrudBuilder<TKey, TEntity> builder,
            Action<DomainEventsConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>> configure)
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            var config = new DomainEventsConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>(builder);
            configure(config);
            return builder;
        }
    }
}