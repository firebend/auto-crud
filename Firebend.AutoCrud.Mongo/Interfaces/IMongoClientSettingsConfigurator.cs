using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoClientSettingsConfigurator
{
    MongoClientSettings Configure(string server, MongoClientSettings settings);
}
