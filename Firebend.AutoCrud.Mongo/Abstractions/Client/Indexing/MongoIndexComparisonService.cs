using System.Collections.Generic;
using System.Linq;
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

            var existingKeys = existingIndexBson["key"].AsBsonDocument;
            var definitionKeys = definition.Keys.Render(collection.DocumentSerializer, new BsonSerializerRegistry());
            var doKeysMatch = existingKeys == definitionKeys;
            var isFullText = definitionKeys.Contains("$**");

            if (!isFullText)
            {
                return doKeysMatch;
            }

            if (existingIndexBson.Contains("weights"))
            {
                try
                {
                    var weightBson = existingIndexBson["weights"].AsBsonDocument;
                    return weightBson.Contains("$**");
                }
                catch
                {
                    //todo: log an error with ILogger
                    //ignore
                }
            }

            return false;
        }

        public bool EnsureUnique<TEntity>(IMongoCollection<TEntity> collection, CreateIndexModel<TEntity>[] definitions)
        {
            var hasDuplicateNames = definitions
                .GroupBy(x => x.Options.Name)
                .Any(x => x.Count() > 1);

            if (hasDuplicateNames)
            {
                return false;
            }

            var hasDuplicateKeys = definitions
                .Select(x => x.Keys)
                .Select(x => x.Render(collection.DocumentSerializer, new BsonSerializerRegistry()))
                .GroupBy(x => x)
                .Any(x => x.Count() > 1);

            if (hasDuplicateKeys)
            {
                return false;
            };

            return true;
        }
    }
}
