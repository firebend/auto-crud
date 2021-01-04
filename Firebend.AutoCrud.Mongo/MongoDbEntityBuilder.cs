using System;
using System.Linq;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.Core.Models.ClassGeneration;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Configuration;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Crud;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Indexing;
using Firebend.AutoCrud.Mongo.Abstractions.Entities;
using Firebend.AutoCrud.Mongo.Implementations;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.AutoCrud.Mongo.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Mongo
{
    public class MongoDbEntityBuilder<TKey, TEntity> : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        public MongoDbEntityBuilder()
        {
            CreateType = typeof(MongoEntityCreateService<TKey, TEntity>);
            ReadType = typeof(MongoEntityReadService<TKey, TEntity>);
            UpdateType = typeof(MongoEntityUpdateService<TKey, TEntity>);

            SearchType = typeof(MongoEntitySearchService<,,>);

            DeleteType = IsActiveEntity
                ? typeof(MongoEntitySoftDeleteService<,>).MakeGenericType(EntityKeyType, EntityType)
                : typeof(MongoEntityDeleteService<TKey, TEntity>);
        }

        public override Type CreateType { get; }

        public override Type ReadType { get; }

        public override Type SearchType { get; }

        public override Type UpdateType { get; }

        public override Type DeleteType { get; }

        public string CollectionName { get; set; }

        public string Database { get; set; }

        public MongoTenantShardMode ShardMode { get; set; }

        protected override void ApplyPlatformTypes()
        {
            RegisterEntityConfiguration();

            if (IsTenantEntity)
            {
                WithRegistration<IMongoCreateClient<TKey, TEntity>>(
                    typeof(MongoTenantCreateClient<,,>).MakeGenericType(EntityKeyType, EntityType, TenantEntityKeyType), false);

                WithRegistration<IMongoReadClient<TKey, TEntity>>(
                    typeof(MongoTenantReadClient<,,>).MakeGenericType(EntityKeyType, EntityType, TenantEntityKeyType), false);

                WithRegistration<IMongoUpdateClient<TKey, TEntity>>(
                    typeof(MongoTenantUpdateClient<,,>).MakeGenericType(EntityKeyType, EntityType, TenantEntityKeyType), false);

                WithRegistration<IMongoDeleteClient<TKey, TEntity>>(
                    typeof(MongoTenantDeleteClient<,,>).MakeGenericType(EntityKeyType, EntityType, TenantEntityKeyType), false);
            }
            else
            {
                WithRegistration<IMongoCreateClient<TKey, TEntity>, MongoCreateClient<TKey, TEntity>>(false);
                WithRegistration<IMongoReadClient<TKey, TEntity>, MongoReadClient<TKey, TEntity>>(false);
                WithRegistration<IMongoUpdateClient<TKey, TEntity>, MongoUpdateClient<TKey, TEntity>>(false);
                WithRegistration<IMongoDeleteClient<TKey, TEntity>, MongoDeleteClient<TKey, TEntity>>(false);
            }

            WithRegistration<IMongoIndexClient<TKey, TEntity>, MongoIndexClient<TKey, TEntity>>(false);
            WithRegistration<IMongoIndexProvider<TEntity>, DefaultIndexProvider<TEntity>>(false);
            WithRegistration<IConfigureCollection<TKey, TEntity>, MongoConfigureCollection<TKey, TEntity>>(false);
            WithRegistration<IConfigureCollection, MongoConfigureCollection<TKey, TEntity>>(false);

            if (IsModifiedEntity)
            {
                var type = typeof(ModifiedIndexProvider<>).MakeGenericType(EntityType);
                WithRegistration<IMongoIndexProvider<TEntity>>(type, false);
            }
            else
            {
                WithRegistration<IMongoIndexProvider<TEntity>, DefaultIndexProvider<TEntity>>(false);
            }

            if (EntityKeyType == typeof(Guid))
            {
                WithRegistration(typeof(IMongoCollectionKeyGenerator<,>).MakeGenericType(EntityKeyType, EntityType),
                    typeof(CombGuidMongoCollectionKeyGenerator<>).MakeGenericType(EntityType),
                    typeof(IMongoCollectionKeyGenerator<,>).MakeGenericType(EntityKeyType, EntityType),
                    false);
            }
        }

        private void RegisterEntityConfiguration()
        {
            var iFaceType = typeof(IMongoEntityConfiguration<TKey, TEntity>);

            if (Registrations.Any(x => x.Key == iFaceType))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(CollectionName))
            {
                throw new Exception("Please provide a collection name for this mongo entity");
            }

            if (string.IsNullOrWhiteSpace(Database))
            {
                throw new Exception("Please provide a database name for this entity.");
            }

            var instance = new MongoEntityConfiguration<TKey, TEntity>(CollectionName, Database, ShardMode);

            if (IsTenantEntity)
            {
                var shardProviderType = typeof(IMongoShardKeyProvider);

                if (!Registrations.ContainsKey(shardProviderType))
                {
                    throw new Exception($"Please register a {nameof(IMongoShardKeyProvider)}");
                }

                if (ShardMode == MongoTenantShardMode.Unknown)
                {
                    throw new Exception($"Please set a {nameof(ShardMode)}");
                }

                WithRegistrationInstance(instance);
                WithRegistration<IMongoEntityConfiguration<TKey, TEntity>, MongoTenantEntityConfiguration<TKey, TEntity>>();
            }
            else
            {
                WithRegistrationInstance(iFaceType, instance);
            }
        }

        public MongoDbEntityBuilder<TKey, TEntity> WithDatabase(string db)
        {
            Database = db;
            return this;
        }

        public MongoDbEntityBuilder<TKey, TEntity> WithCollection(string collection)
        {
            CollectionName = collection;
            return this;
        }

        public MongoDbEntityBuilder<TKey, TEntity> WithDefaultDatabase(string db)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new Exception("Please provide a database name.");
            }

            if (string.IsNullOrWhiteSpace(Database))
            {
                Database = db;
            }

            const string signature = "DefaultDb";

            var iFaceType = typeof(IMongoDefaultDatabaseSelector);

            var defaultDbField = new PropertySet { Name = nameof(IMongoDefaultDatabaseSelector.DefaultDb), Type = typeof(string), Value = db, Override = true };

            WithDynamicClass(iFaceType,
                new DynamicClassRegistration
                {
                    Interface = iFaceType,
                    Properties = new[] { defaultDbField },
                    Signature = signature,
                    Lifetime = ServiceLifetime.Singleton
                });

            return this;
        }

        public MongoDbEntityBuilder<TKey, TEntity> WithFullTextSearch()
        {
            if (IsModifiedEntity)
            {
                var type = typeof(ModifiedFullTextIndexProvider<>).MakeGenericType(EntityType);
                WithRegistration<IMongoIndexProvider<TEntity>>(type);
            }
            else
            {
                WithRegistration<IMongoIndexProvider<TEntity>, FullTextIndexProvider<TEntity>>();
            }

            return this;
        }

        public MongoDbEntityBuilder<TKey, TEntity> WithShardMode(MongoTenantShardMode mode)
        {
            ShardMode = mode;
            return this;
        }

        public MongoDbEntityBuilder<TKey, TEntity> WithShardKeyProvider<TShardKeyProvider>()
            where TShardKeyProvider : IMongoShardKeyProvider
        {
            WithRegistration<IMongoShardKeyProvider, TShardKeyProvider>();
            return this;
        }
    }
}
