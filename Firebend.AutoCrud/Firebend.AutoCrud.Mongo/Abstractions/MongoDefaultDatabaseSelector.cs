using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions
{
    public abstract class MongoDefaultDatabaseSelector : IMongoDefaultDatabaseSelector
    {
        private readonly IMongoClient _client;
        private readonly string _defaultName;

        public MongoDefaultDatabaseSelector(IMongoClient client, string defaultName)
        {
            _client = client;
            _defaultName = defaultName;
        }

        public IMongoDatabase GetDefaultDb()
        {
            return _client.GetDatabase(_defaultName);
        }
    }
}