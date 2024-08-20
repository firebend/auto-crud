using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoReadPreferenceService
{
    ReadPreferenceMode? GetMode();
    void SetMode(ReadPreferenceMode mode);
}
