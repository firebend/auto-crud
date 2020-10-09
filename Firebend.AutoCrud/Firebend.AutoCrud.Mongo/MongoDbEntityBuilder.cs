using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.ClassGeneration;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.ClassGeneration;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Crud;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Indexing;
using Firebend.AutoCrud.Mongo.Abstractions.Entities;
using Firebend.AutoCrud.Mongo.Configuration;
using Firebend.AutoCrud.Mongo.Implementations;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo
{
    public class MongoDbEntityBuilder : EntityCrudBuilder
    {
        private readonly IDynamicClassGenerator _generator;
        public override Type CreateType { get; } = typeof(MongoEntityCreateService<,>);
        
        public override Type ReadType { get; } = typeof(MongoEntityReadService<,>);
        
        public override Type SearchType { get; } = typeof(MongoEntitySearchService<,,>);
        
        public override Type UpdateType { get; } = typeof(MongoEntityUpdateService<,>);
        
        public override Type DeleteType { get; } = typeof(MongoEntityDeleteService<,>);

        public override Type SoftDeleteType { get; } = typeof(MongoEntitySoftDeleteService<,>);
        
        public string CollectionName { get; set; }
        
        public string Database { get; set; }

        public MongoDbEntityBuilder(IDynamicClassGenerator generator)
        {
            _generator = generator;
        }

        public override void ApplyPlatformTypes()
        {
            this.WithRegistration(typeof(IMongoCreateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(MongoCreateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IMongoCreateClient<,>).MakeGenericType(EntityKeyType, EntityType));
            
            this.WithRegistration(typeof(IMongoReadClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(MongoReadClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IMongoReadClient<,>).MakeGenericType(EntityKeyType, EntityType));
            
            this.WithRegistration(typeof(IMongoUpdateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(MongoUpdateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IMongoUpdateClient<, >).MakeGenericType(EntityKeyType, EntityType));
            
            this.WithRegistration(typeof(IMongoDeleteClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(MongoDeleteClient<, >).MakeGenericType(EntityKeyType, EntityType),
                typeof(IMongoDeleteClient<, >).MakeGenericType(EntityKeyType, EntityType));
            
            this.WithRegistration(typeof(IMongoIndexClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(MongoIndexClient<, >).MakeGenericType(EntityKeyType, EntityType),
                typeof(IMongoIndexClient<,>).MakeGenericType(EntityKeyType, EntityType));
            
            this.WithRegistration(typeof(IMongoIndexProvider<>).MakeGenericType(EntityType),
                typeof(DefaultIndexProvider<>).MakeGenericType(EntityType),
                typeof(IMongoIndexProvider<>).MakeGenericType(EntityType));

            this.WithRegistration(typeof(IConfigureCollection<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(MongoConfigureCollection<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IConfigureCollection<,>).MakeGenericType(EntityKeyType, EntityType));

            this.WithRegistration(typeof(IEntityDefaultOrderByProvider<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(DefaultEntityDefaultOrderByProvider<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IEntityDefaultOrderByProvider<,>).MakeGenericType(EntityKeyType, EntityType));
        }

        public override void Build()
        {
            base.Build();
            ApplyPlatformTypes();
            RegisterCollectionNameInterfaceType();
            
            if (EntityKeyType == typeof(Guid))
            {
                this.WithRegistration(typeof(IMongoCollectionKeyGenerator<,>).MakeGenericType(EntityKeyType, EntityType),
                    typeof(CombGuidMongoCollectionKeyGenerator<>).MakeGenericType(EntityType),
                    typeof(IMongoCollectionKeyGenerator<,>).MakeGenericType(EntityKeyType, EntityType));
            }
        }
        
        private void RegisterCollectionNameInterfaceType()
        {
            if (string.IsNullOrWhiteSpace(CollectionName))
            {
                throw new Exception("Please provide a collection name for this mongo entity");
            }

            if (string.IsNullOrWhiteSpace(Database))
            {
                throw new Exception("Please provide a database name for this entity.");
            }
            
            var collectionNameSignature = $"{EntityType.Name}_{CollectionName}_CollectionName";
            
            var collectionNameInterfaceType = typeof(IMongoEntityConfiguration<,>).MakeGenericType(EntityKeyType, EntityType);
            var collectionNameInterface = _generator.GenerateInterface(collectionNameInterfaceType, $"I{collectionNameSignature}");

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

            var collectionNameImplementation = _generator.ImplementInterface(collectionNameInterface,
                collectionNameSignature,
                new[]
                {
                    collectionNameField,
                    databaseField
                });

            this.WithRegistration(collectionNameInterface, collectionNameImplementation.GetType());
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
    }
}