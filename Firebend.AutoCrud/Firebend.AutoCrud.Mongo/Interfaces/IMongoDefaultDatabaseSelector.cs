using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoDefaultDatabaseSelector
    {
        IMongoDatabase GetDefaultDb();
    }
}