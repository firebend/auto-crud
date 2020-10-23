using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Io
{
    public static class Extensions
    {
        public static EntityCrudBuilder<TKey, TEntity> AddIo<TKey, TEntity>(this EntityCrudBuilder<TKey, TEntity> builder,
            Action<IoConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>> configure = null)
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            var config = new IoConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>(builder);
            configure?.Invoke(config);
            return builder;
        }
    }
}