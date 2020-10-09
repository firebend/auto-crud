using System;
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
            base.ApplyPlatformTypes();
            
            this.WithRegistration(typeof(IMongoCreateClient<,>),
                typeof(MongoCreateClient<,>),
                typeof(IMongoCreateClient<,>),
                EntityKeyType, EntityType);
            
            this.WithRegistration(typeof(IMongoReadClient<,>),
                typeof(MongoReadClient<,>),
                typeof(IMongoReadClient<,>),
                EntityKeyType, EntityType);
            
            this.WithRegistration(typeof(IMongoUpdateClient<, >),
                typeof(MongoDeleteClient<, >),
                typeof(IMongoDeleteClient<, >),
                EntityKeyType, EntityType);
            
            this.WithRegistration(typeof(IMongoIndexClient<, >),
                typeof(MongoIndexClient<, >),
                typeof(IMongoIndexClient<,>),
                EntityKeyType, EntityType);
            
            this.WithRegistration(typeof(IMongoIndexProvider<>),
                typeof(DefaultIndexProvider<>),
                typeof(IMongoIndexProvider<>),
                EntityType);

            this.WithRegistration(typeof(IConfigureCollection<,>),
                typeof(MongoConfigureCollection<,>),
                typeof(IConfigureCollection<,>),
                EntityKeyType, EntityType);

            this.WithRegistration(typeof(IEntityDefaultOrderByProvider<,>),
                typeof(DefaultEntityDefaultOrderByProvider<,>),
                typeof(IEntityDefaultOrderByProvider<,>),
                EntityKeyType, EntityType);
        }

        public override void Build()
        {
            base.Build();
            RegisterCollectionNameInterfaceType();
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
    }
}