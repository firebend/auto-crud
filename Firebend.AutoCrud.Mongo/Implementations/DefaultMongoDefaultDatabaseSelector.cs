using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Implementations;

public class DefaultMongoDefaultDatabaseSelector : IMongoDefaultDatabaseSelector
{
    public DefaultMongoDefaultDatabaseSelector(string defaultDb)
    {
        DefaultDb = defaultDb;
    }

    public string DefaultDb { get; }
}
