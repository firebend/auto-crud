using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public class MongoEntityTransactionFactory<TKey, TEntity> :
        Firebend.AutoCrud.Mongo.Abstractions.Client.MongoClientBase, IEntityTransactionFactory<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IEntityTransactionOutbox _outbox;

        public MongoEntityTransactionFactory(IMongoClient client,
            ILoggerFactory loggerFactory,
            IEntityTransactionOutbox outbox,
            IMongoRetryService retryService) :
            base(client, loggerFactory.CreateLogger<MongoEntityTransactionFactory<TKey, TEntity>>(), retryService)
        {
            _outbox = outbox;
        }

        public Task<int> GetDbContextHashCode()
        {
            var hashCode = Client.Settings.GetHashCode();
            return Task.FromResult(hashCode);
        }

        public async Task<IEntityTransaction> StartTransactionAsync(CancellationToken cancellationToken)
        {
            var transactionOptions = new TransactionOptions(ReadConcern.Snapshot, writeConcern: WriteConcern.WMajority);
            var sessionOptions = new ClientSessionOptions { DefaultTransactionOptions = transactionOptions };
            var session = await Client.StartSessionAsync(sessionOptions, cancellationToken);
            session.StartTransaction(transactionOptions);
            return new MongoEntityTransaction(session, _outbox);
        }

        public bool ValidateTransaction(IEntityTransaction transaction)
        {
            if (transaction is not MongoEntityTransaction mongoTransaction)
            {
                return false;
            }

            return mongoTransaction.ClientSessionHandle.IsInTransaction;
        }
    }
}
