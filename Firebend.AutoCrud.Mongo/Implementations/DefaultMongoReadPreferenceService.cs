using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations;

public class DefaultMongoReadPreferenceService : IMongoReadPreferenceService
{
    private ReadPreferenceMode? _readPreference;

    public ReadPreferenceMode? GetMode() => _readPreference;

    public void SetMode(ReadPreferenceMode mode) => _readPreference = mode;
}
