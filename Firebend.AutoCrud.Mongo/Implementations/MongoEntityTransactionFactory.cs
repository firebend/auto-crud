using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Abstractions.Client;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public class MongoEntityTransactionFactory<TKey, TEntity> :
        MongoClientBase<TKey, TEntity>, IEntityTransactionFactory<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IEntityTransactionOutbox _outbox;

        public MongoEntityTransactionFactory(IMongoClientFactory<TKey, TEntity> factory,
            ILoggerFactory loggerFactory,
            IEntityTransactionOutbox outbox,
            IMongoRetryService retryService) :
            base(factory, loggerFactory.CreateLogger<MongoEntityTransactionFactory<TKey, TEntity>>(), retryService)
        {
            _outbox = outbox;
        }

        public Task<string> GetDbContextHashCode()
        {
            var hashCode = Client.Settings.GetHashCode();
            return Task.FromResult($"mongo_{hashCode}");
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
