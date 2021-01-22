using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoClientFactory
    {
        IMongoClient CreateClient(string connectionString, bool enableLogging);
    }
}
