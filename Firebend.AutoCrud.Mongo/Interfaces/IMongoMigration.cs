using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Mongo.Models;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoMigration
{
    public MongoDbMigrationVersion Version { get; }

    public Task<bool> ApplyMigrationAsync(CancellationToken cancellationToken);
}
