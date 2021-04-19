using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public class MongoEntityTransactionFactory<TKey, TEntity> : Firebend.AutoCrud.Mongo.Abstractions.Client.MongoClientBase, IEntityTransactionFactory<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IEntityTransactionOutbox _outbox;

        public MongoEntityTransactionFactory(IMongoClient client, ILoggerFactory loggerFactory, IEntityTransactionOutbox outbox) :
            base(client, loggerFactory.CreateLogger<MongoEntityTransactionFactory<TKey, TEntity>>())
        {
            _outbox = outbox;
        }

        public async Task<IEntityTransaction> StartTransactionAsync(CancellationToken cancellationToken)
        {
            var transactionOptions = new TransactionOptions(ReadConcern.Snapshot, writeConcern: WriteConcern.WMajority);
            var sessionOptions = new ClientSessionOptions { DefaultTransactionOptions = transactionOptions };
            var session = await Client.StartSessionAsync(sessionOptions, cancellationToken);
            session.StartTransaction(transactionOptions);
            return new MongoEntityTransaction(session, _outbox);
        }
    }
}
