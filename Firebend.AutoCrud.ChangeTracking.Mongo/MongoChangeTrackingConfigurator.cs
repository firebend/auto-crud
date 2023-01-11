using System;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Abstractions.Client;
using Firebend.AutoCrud.Mongo.Implementations;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.ChangeTracking.Mongo;

public class MongoChangeTrackingConfigurator<TBuilder, TKey, TEntity> : EntityBuilderConfigurator<TBuilder, TKey, TEntity>
    where TBuilder : EntityCrudBuilder<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    public MongoChangeTrackingConfigurator(TBuilder builder) : base(builder)
    {
    }

    //TODO TS: add docs
    public MongoChangeTrackingConfigurator<TBuilder, TKey, TEntity> WithConnectionStringProvider<TConnectionStringProvider>()
        where TConnectionStringProvider : class, IMongoConnectionStringProvider<Guid, ChangeTrackingEntity<TKey, TEntity>>
    {
        Builder.WithRegistration<IMongoConnectionStringProvider<Guid, ChangeTrackingEntity<TKey, TEntity>>, TConnectionStringProvider>();
        Builder
            .WithRegistration<IMongoClientFactory<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                MongoClientFactory<Guid, ChangeTrackingEntity<TKey, TEntity>>>();

        return this;
    }

    public MongoChangeTrackingConfigurator<TBuilder, TKey, TEntity> WithConnectionString(string connectionString)
    {
        var instance =
            new DefaultMongoConnectionStringProvider<Guid, ChangeTrackingEntity<TKey, TEntity>>(connectionString);

        Builder.WithRegistrationInstance<IMongoConnectionStringProvider<Guid, ChangeTrackingEntity<TKey, TEntity>>>(instance);
        Builder.WithRegistration<
            IMongoClientFactory<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                MongoClientFactory<Guid, ChangeTrackingEntity<TKey, TEntity>>>();

        return this;
    }
}
