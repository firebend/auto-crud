#region

using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Mongo.Models;

#endregion

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoMigration
    {
        MongoDbMigrationVersion Version { get; }

        Task<bool> ApplyMigrationAsync(CancellationToken cancellationToken);
    }
}