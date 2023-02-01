using System.Threading.Tasks;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Implementations;

public class MongoMigrationConnectionStringProvider : IMongoMigrationConnectionStringProvider
{
    private readonly string _connectionString;

    public MongoMigrationConnectionStringProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public string GetConnectionString() => _connectionString;
}
