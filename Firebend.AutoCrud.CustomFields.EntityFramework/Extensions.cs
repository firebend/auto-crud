using System;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework;

namespace Firebend.AutoCrud.CustomFields.EntityFramework
{
    public static class Extensions
    {
        public static EntityFrameworkEntityBuilder<TKey, TEntity> AddCustomFields<TKey, TEntity>(
            this EntityFrameworkEntityBuilder<TKey, TEntity> builder,
            Action<EfCustomFieldsConfigurator<EntityFrameworkEntityBuilder<TKey, TEntity>, TKey, TEntity>> configure = null)
            where TKey : struct
            where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>, new()
        {
            var configurator = new EfCustomFieldsConfigurator<EntityFrameworkEntityBuilder<TKey, TEntity>, TKey, TEntity>(builder);

            if (configure == null)
            {
                configurator.AddCustomFields();
            }
            else
            {
                configure(configurator);
            }

            return builder;
        }
    }
}
