using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Mongo.Abstractions.Client;
using Firebend.AutoCrud.Mongo.Configuration;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.CustomFields.Mongo.Abstractions;

public class AbstractMongoCustomFieldsCreateService<TKey, TEntity> :
    MongoClientBaseEntity<TKey, TEntity>,
    ICustomFieldsCreateService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
{
    public AbstractMongoCustomFieldsCreateService(IMongoClient client,
        ILogger<AbstractMongoCustomFieldsCreateService<TKey, TEntity>> logger,
        IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
        IMongoRetryService mongoRetryService) : base(client, logger, entityConfiguration, mongoRetryService)
    {
    }

    public Task<CustomFieldsEntity<TKey>>
        CreateAsync(TKey rootEntityKey, CustomFieldsEntity<TKey> customField, CancellationToken cancellationToken = default) =>
        CreateAsync(rootEntityKey, customField, null, cancellationToken);

    public async Task<CustomFieldsEntity<TKey>> CreateAsync(TKey rootEntityKey,
        CustomFieldsEntity<TKey> customField,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        if (customField.Id == default)
        {
            customField.Id = MongoIdGeneratorComb.NewCombGuid(Guid.NewGuid(), DateTime.UtcNow);
        }

        customField.CreatedDate = DateTimeOffset.UtcNow;
        customField.ModifiedDate = DateTimeOffset.UtcNow;

        var filters = await BuildFiltersAsync(x => x.Id.Equals(rootEntityKey), cancellationToken).ConfigureAwait(false);
        var filtersDefinition = Builders<TEntity>.Filter.Where(filters);
        var mongoCollection = GetCollection();
        var updateDefinition = Builders<TEntity>.Update.Push(x => x.CustomFields, customField);

        if (typeof(IModifiedEntity).IsAssignableFrom(typeof(TEntity)))
        {
            updateDefinition = Builders<TEntity>.Update.Combine(updateDefinition,
                    Builders<TEntity>.Update.Set(nameof(IModifiedEntity.ModifiedDate), DateTimeOffset.UtcNow));
        }

        var session = UnwrapSession(entityTransaction);

        TEntity result;

        if (session is not null)
        {
            result = await mongoCollection.FindOneAndUpdateAsync(session,
                filtersDefinition,
                updateDefinition,
                new FindOneAndUpdateOptions<TEntity> { ReturnDocument = ReturnDocument.After },
                cancellationToken);
        }
        else
        {
            result = await mongoCollection.FindOneAndUpdateAsync(filtersDefinition,
                updateDefinition,
                new FindOneAndUpdateOptions<TEntity> { ReturnDocument = ReturnDocument.After },
                cancellationToken);
        }

        //todo: pub domain event
        return result?.CustomFields?.FirstOrDefault(x => x.Id == customField.Id);
    }
}
