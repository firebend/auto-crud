#region

using System.Collections.Concurrent;

#endregion

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Indexing
{
    internal class MongoIndexClientConfigurations
    {
        public static readonly ConcurrentDictionary<string, bool> Configurations = new ConcurrentDictionary<string, bool>();
    }
}