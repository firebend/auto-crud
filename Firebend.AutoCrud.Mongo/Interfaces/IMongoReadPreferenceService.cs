using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoReadPreferenceService
{
    public ReadPreferenceMode? GetMode();
    public void SetMode(ReadPreferenceMode mode);
}
