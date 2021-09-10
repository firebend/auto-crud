using System.Collections.Generic;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.JsonPatch.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Indexing
{
    public class MongoIndexComparisonService : IMongoIndexComparisonService
    {
        public bool DoesIndexMatch<TEntity>(IMongoCollection<TEntity> collection, BsonDocument existingIndexBson, CreateIndexModel<TEntity> definition)
        {
            if (existingIndexBson == null)
            {
                return false;
            }

            var doesNameMatch = existingIndexBson["name"].AsString.EqualsIgnoreCaseAndWhitespace(definition.Options.Name);

            if (!doesNameMatch)
            {
                return false;
            }

            var doKeysMatch = existingIndexBson["key"].AsBsonDocument.Equals(definition.Keys.Render(collection.DocumentSerializer, new BsonSerializerRegistry()));

            return doesNameMatch;
        }

        public bool EnsureUnique<TEntity>(IEnumerable<CreateIndexModel<TEntity>> definitions) => throw new System.NotImplementedException();
    }
}
