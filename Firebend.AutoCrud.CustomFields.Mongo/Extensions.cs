using System;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo;

namespace Firebend.AutoCrud.CustomFields.Mongo;

public static class Extensions
{
    public static MongoDbEntityBuilder<TKey, TEntity> AddCustomFields<TKey, TEntity>(
        this MongoDbEntityBuilder<TKey, TEntity> builder,
        Action<MongoCustomFieldsConfigurator<MongoDbEntityBuilder<TKey, TEntity>, TKey, TEntity>> configure = null)
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>, new()
    {
        using var configurator = new MongoCustomFieldsConfigurator<MongoDbEntityBuilder<TKey, TEntity>, TKey, TEntity>(builder);

        if (configure == null)
        {
            configurator.WithCustomFields();
        }
        else
        {
            configure(configurator);
        }

        return builder;
    }
}
