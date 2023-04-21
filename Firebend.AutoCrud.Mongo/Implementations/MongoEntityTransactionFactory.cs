using System;
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
    public static class MongoEntityTransactionFactoryDefaults
    {
#pragma warning disable CA2211, IDE1006
        public static TransactionOptions TransactionOptions;
        public static ClientSessionOptions SessionOptions;
#pragma warning enable CA2211, IDE1006

        static MongoEntityTransactionFactoryDefaults()
        {
            TransactionOptions = new TransactionOptions(
                ReadConcern.Local,
                readPreference: ReadPreference.Primary,
                writeConcern: WriteConcern.WMajority,
                maxCommitTime: TimeSpan.FromMinutes(5));

            SessionOptions = new ClientSessionOptions
            {
                DefaultTransactionOptions = TransactionOptions,
            };
        }
    }
    public class MongoEntityTransactionFactory<TKey, TEntity> :
        MongoClientBase<TKey, TEntity>, IEntityTransactionFactory<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IEntityTransactionOutbox _outbox;
        private readonly ILoggerFactory _loggerFactory;

        public MongoEntityTransactionFactory(IMongoClientFactory<TKey, TEntity> factory,
            ILoggerFactory loggerFactory,
            IEntityTransactionOutbox outbox,
            IMongoRetryService retryService) :
            base(factory, loggerFactory.CreateLogger<MongoEntityTransactionFactory<TKey, TEntity>>(), retryService)
        {
            _outbox = outbox;
            _loggerFactory = loggerFactory;
        }

        public async Task<string> GetDbContextHashCode()
        {
            var client = await GetClientAsync();
            var hashCode = client.Settings.GetHashCode();
            return $"mongo_{hashCode}";
        }

        public async Task<IEntityTransaction> StartTransactionAsync(CancellationToken cancellationToken)
        {
            var client = await GetClientAsync();
            var session = await client.StartSessionAsync(MongoEntityTransactionFactoryDefaults.SessionOptions, cancellationToken);
            session.StartTransaction(MongoEntityTransactionFactoryDefaults.TransactionOptions);
            return new MongoEntityTransaction(session, _outbox, MongoRetryService, _loggerFactory);
        }

        public bool ValidateTransaction(IEntityTransaction transaction)
            => transaction is MongoEntityTransaction mongoTransaction && mongoTransaction.ClientSessionHandle.IsInTransaction;
    }
}
