using System.Collections.Concurrent;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Indexing
{
    internal class MongoIndexClientConfigurations
    {
        public static readonly ConcurrentDictionary<string, bool> Configurations = new();
    }
}
