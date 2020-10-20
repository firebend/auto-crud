using System;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.Core.Models.ClassGeneration;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Configuration;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Crud;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Indexing;
using Firebend.AutoCrud.Mongo.Abstractions.Entities;
using Firebend.AutoCrud.Mongo.Implementations;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Mongo
{
    public class MongoDbEntityBuilder : EntityCrudBuilder
    {
        public override Type CreateType { get; } = typeof(MongoEntityCreateService<,>);

        public override Type ReadType { get; } = typeof(MongoEntityReadService<,>);

        public override Type SearchType { get; } = typeof(MongoEntitySearchService<,,>);

        public override Type UpdateType { get; } = typeof(MongoEntityUpdateService<,>);

        public override Type DeleteType { get; } = typeof(MongoEntityDeleteService<,>);

        public override Type SoftDeleteType { get; } = typeof(MongoEntitySoftDeleteService<,>);

        public string CollectionName { get; set; }

        public string Database { get; set; }

        protected override void ApplyPlatformTypes()
        {
            RegisterEntityConfiguration();

            this.WithRegistration(typeof(IMongoCreateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(MongoCreateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IMongoCreateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                false);

            this.WithRegistration(typeof(IMongoReadClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(MongoReadClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IMongoReadClient<,>).MakeGenericType(EntityKeyType, EntityType),
                false);

            this.WithRegistration(typeof(IMongoUpdateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(MongoUpdateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IMongoUpdateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                false);

            this.WithRegistration(typeof(IMongoDeleteClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(MongoDeleteClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IMongoDeleteClient<,>).MakeGenericType(EntityKeyType, EntityType),
                false);

            this.WithRegistration(typeof(IMongoIndexClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(MongoIndexClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IMongoIndexClient<,>).MakeGenericType(EntityKeyType, EntityType),
                false);

            this.WithRegistration(typeof(IMongoIndexProvider<>).MakeGenericType(EntityType),
                typeof(DefaultIndexProvider<>).MakeGenericType(EntityType),
                typeof(IMongoIndexProvider<>).MakeGenericType(EntityType),
                false);

            this.WithRegistration(typeof(IConfigureCollection<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(MongoConfigureCollection<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IConfigureCollection<,>).MakeGenericType(EntityKeyType, EntityType),
                false);

            this.WithRegistration(typeof(IConfigureCollection),
                typeof(MongoConfigureCollection<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IConfigureCollection),
                false);

            this.WithRegistration(typeof(IEntityDefaultOrderByProvider<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(DefaultEntityDefaultOrderByProvider<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IEntityDefaultOrderByProvider<,>).MakeGenericType(EntityKeyType, EntityType),
                false);

            this.WithRegistration<MongoDbEntityBuilder, IEntityDomainEventPublisher, DefaultEntityDomainEventPublisher>(false);

            if (EntityKeyType == typeof(Guid))
                this.WithRegistration(typeof(IMongoCollectionKeyGenerator<,>).MakeGenericType(EntityKeyType, EntityType),
                    typeof(CombGuidMongoCollectionKeyGenerator<>).MakeGenericType(EntityType),
                    typeof(IMongoCollectionKeyGenerator<,>).MakeGenericType(EntityKeyType, EntityType),
                    false);
        }

        private void RegisterEntityConfiguration()
        {
            if (string.IsNullOrWhiteSpace(CollectionName)) throw new Exception("Please provide a collection name for this mongo entity");

            if (string.IsNullOrWhiteSpace(Database)) throw new Exception("Please provide a database name for this entity.");

            var signature = $"{EntityType.Name}_{CollectionName}_CollectionName";

            var iFaceType = typeof(IMongoEntityConfiguration<,>).MakeGenericType(EntityKeyType, EntityType);

            var collectionNameField = new PropertySet
            {
                Name = nameof(IMongoEntityConfiguration<Guid, FooEntity>.CollectionName),
                Type = typeof(string),
                Value = CollectionName,
                Override = true
            };

            var databaseField = new PropertySet
            {
                Name = nameof(IMongoEntityConfiguration<Guid, FooEntity>.DatabaseName),
                Type = typeof(string),
                Value = Database,
                Override = true
            };

            this.WithDynamicClass(iFaceType, new DynamicClassRegistration
            {
                Interface = iFaceType,
                Properties = new[] {databaseField, collectionNameField},
                Signature = signature,
                Lifetime = ServiceLifetime.Singleton
            });
        }

        public MongoDbEntityBuilder WithDatabase(string db)
        {
            Database = db;
            return this;
        }

        public MongoDbEntityBuilder WithCollection(string collection)
        {
            CollectionName = collection;
            return this;
        }

        public MongoDbEntityBuilder WithDefaultDatabase(string db)
        {
            if (string.IsNullOrWhiteSpace(db)) throw new Exception("Please provide a database name.");

            if (string.IsNullOrWhiteSpace(Database)) Database = db;

            var signature = "DefaultDb";

            var iFaceType = typeof(IMongoDefaultDatabaseSelector);

            var defaultDbField = new PropertySet
            {
                Name = nameof(IMongoDefaultDatabaseSelector.DefaultDb),
                Type = typeof(string),
                Value = db,
                Override = true
            };

            return this.WithDynamicClass(iFaceType, new DynamicClassRegistration
            {
                Interface = iFaceType,
                Properties = new [] { defaultDbField },
                Signature = signature,
                Lifetime = ServiceLifetime.Singleton
            });
        }

        public MongoDbEntityBuilder WithFullTextSearch()
        {
            return this.WithRegistration(typeof(IMongoIndexProvider<>).MakeGenericType(EntityType),
                typeof(FullTextIndexProvider<>).MakeGenericType(EntityType),
                typeof(IMongoIndexProvider<>).MakeGenericType(EntityType));
        }
    }
}