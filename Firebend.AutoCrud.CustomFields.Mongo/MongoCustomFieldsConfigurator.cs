using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.CustomFields.Mongo.Abstractions;
using Firebend.AutoCrud.Mongo;

namespace Firebend.AutoCrud.CustomFields.Mongo;

public class MongoCustomFieldsConfigurator<TBuilder, TKey, TEntity> : EntityBuilderConfigurator<TBuilder, TKey, TEntity>
    where TBuilder : MongoDbEntityBuilder<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>, new()
{
    public MongoCustomFieldsConfigurator(TBuilder builder) : base(builder)
    {
    }

    public MongoCustomFieldsConfigurator<TBuilder, TKey, TEntity> WithSearchHandler<TService>()
    {
        var serviceType = typeof(TService);
        var registrationType = typeof(IEntitySearchHandler<TKey, TEntity, CustomFieldsSearchRequest>);
        Builder.WithRegistration(registrationType, serviceType,
            registrationType);
        return this;
    }

    public MongoCustomFieldsConfigurator<TBuilder, TKey, TEntity> WithCustomFields()
    {
        Builder.WithRegistration<ICustomFieldsCreateService<TKey, TEntity>,
            AbstractMongoCustomFieldsCreateService<TKey, TEntity>>();
        Builder
            .WithRegistration<ICustomFieldsDeleteService<TKey, TEntity>,
                AbstractMongoCustomFieldsDeleteService<TKey, TEntity>>();
        Builder
            .WithRegistration<ICustomFieldsUpdateService<TKey, TEntity>,
                AbstractMongoCustomFieldsUpdateService<TKey, TEntity>>();
        Builder
            .WithRegistration<ICustomFieldsSearchService<TKey, TEntity>,
                AbstractMongoCustomFieldsSearchService<TKey, TEntity>>();
        return this;
    }
}
