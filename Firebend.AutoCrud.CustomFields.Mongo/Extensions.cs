using Firebend.AutoCrud.Core.Abstractions.Services;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.CustomFields.Mongo.Abstractions;
using Firebend.AutoCrud.Mongo;

namespace Firebend.AutoCrud.CustomFields.Mongo
{
    public static class Extensions
    {
        /// <summary>
        /// Adds custom fields functionality to this entity.
        /// </summary>
        /// <typeparam name="TKey">
        /// The entity's key type.
        /// </typeparam>
        /// <typeparam name="TEntity">
        /// The entity type.
        /// </typeparam>
        /// <returns>
        /// The Mongo Builder.
        /// </returns>
        public static MongoDbEntityBuilder<TKey, TEntity> AddCustomFields<TKey, TEntity>(
            this MongoDbEntityBuilder<TKey, TEntity> builder)
            where TKey : struct
            where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>, new()
        {
            builder.WithRegistration<ICustomFieldsCreateService<TKey, TEntity>, AbstractMongoCustomFieldsCreateService<TKey, TEntity>>();
            builder.WithRegistration<ICustomFieldsDeleteService<TKey, TEntity>, AbstractCustomFieldsAlterService<TKey, TEntity>>();
            builder.WithRegistration<ICustomFieldsUpdateService<TKey, TEntity>, AbstractMongoCustomFieldsUpdateService<TKey, TEntity>>();

            builder.WithRegistration<ICustomFieldsSearchService<TKey, TEntity>, AbstractMongoCustomFieldsSearchService<TKey, TEntity>>();

            return builder;
        }
    }
}
