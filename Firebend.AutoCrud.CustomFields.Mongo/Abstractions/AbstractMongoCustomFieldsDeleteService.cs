using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Mongo.Abstractions.Client;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.CustomFields.Mongo.Abstractions;

public class AbstractMongoCustomFieldsDeleteService<TKey, TEntity> :
    MongoClientBaseEntity<TKey, TEntity>,
    ICustomFieldsDeleteService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
{
    public AbstractMongoCustomFieldsDeleteService(IMongoClient client,
        ILogger<AbstractMongoCustomFieldsDeleteService<TKey, TEntity>> logger,
        IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
        IMongoRetryService mongoRetryService) : base(client, logger, entityConfiguration, mongoRetryService)
    {
    }

    public Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey, Guid key, CancellationToken cancellationToken = default)
        => DeleteAsync(rootEntityKey, key, null, cancellationToken);

    public async Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey,
        Guid key,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        var filters = await BuildFiltersAsync(x => x.Id.Equals(rootEntityKey), cancellationToken).ConfigureAwait(false);
        var filtersDefinition = Builders<TEntity>.Filter.Where(filters)
                                & Builders<TEntity>.Filter.ElemMatch(x => x.CustomFields, cf => cf.Id == key);

        var mongoCollection = GetCollection();
        var updateDefinition = Builders<TEntity>.Update.PullFilter(x => x.CustomFields, x => x.Id == key);

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
                new FindOneAndUpdateOptions<TEntity> { ReturnDocument = ReturnDocument.Before },
                cancellationToken);
        }
        else
        {
            result = await mongoCollection.FindOneAndUpdateAsync(filtersDefinition,
                updateDefinition,
                new FindOneAndUpdateOptions<TEntity> { ReturnDocument = ReturnDocument.Before },
                cancellationToken);
        }

        return result?.CustomFields?.FirstOrDefault(x => x.Id == key);
    }
}
