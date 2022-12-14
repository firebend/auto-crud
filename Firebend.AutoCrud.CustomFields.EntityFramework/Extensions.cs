using System;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework;

namespace Firebend.AutoCrud.CustomFields.EntityFramework
{
    public static class Extensions
    {
        /// <summary>
        /// Adds custom fields functionality to this entity.
        /// </summary>
        /// <param name="builder">
        /// The builder to configure custom fields against.
        /// </param>
        /// <param name="configure">
        /// A call back to add any additional configuration.
        /// </param>
        /// <typeparam name="TKey">
        /// The entity's key type.
        /// </typeparam>
        /// <typeparam name="TEntity">
        /// The entity type.
        /// </typeparam>
        /// <returns>
        /// The EF Builder.
        /// </returns>
        public static EntityFrameworkEntityBuilder<TKey, TEntity> AddCustomFields<TKey, TEntity>(
            this EntityFrameworkEntityBuilder<TKey, TEntity> builder,
            Action<EfCustomFieldsConfigurator<EntityFrameworkEntityBuilder<TKey, TEntity>, TKey, TEntity>> configure = null)
            where TKey : struct
            where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>, new()
        {
            using var configurator = new EfCustomFieldsConfigurator<EntityFrameworkEntityBuilder<TKey, TEntity>, TKey, TEntity>(builder);

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
