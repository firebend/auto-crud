using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoClientSettingsConfigurator
{
    public MongoClientSettings Configure(string server, MongoClientSettings settings);
}
