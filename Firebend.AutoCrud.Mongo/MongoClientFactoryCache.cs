using System.Collections.Concurrent;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo;

public static class MongoClientFactoryCache
{
    public static readonly ConcurrentDictionary<string, IMongoClient> MongoClients = new();
}
