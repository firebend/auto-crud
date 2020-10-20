using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web
{
    public static class ControllerEntityBuilderExtensions
    { 
        public static EntityCrudBuilder<TKey, TEntity> AddControllers<TKey, TEntity>(this EntityCrudBuilder<TKey, TEntity> builder,
            Action<ControllerConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>> configure)
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            configure(new ControllerConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>(builder));
            return builder;
        }
    }
}