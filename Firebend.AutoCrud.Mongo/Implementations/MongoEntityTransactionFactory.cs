using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoClientBase = Firebend.AutoCrud.Mongo.Abstractions.Client.MongoClientBase;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public class MongoEntityTransactionFactory<TKey, TEntity> : MongoClientBase, IEntityTransactionFactory<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public MongoEntityTransactionFactory(IMongoClient client, ILogger logger) : base(client, logger)
        {
        }

        public async Task<IEntityTransaction> StartTransactionAsync(CancellationToken cancellationToken)
        {
            var session = await Client.StartSessionAsync(null, cancellationToken);
            session.StartTransaction();
            return new MongoEntityTransaction(session);
        }
    }
}
