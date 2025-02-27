using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoEntityConfigurationTenantTransformService<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    public string GetCollection(IMongoEntityDefaultConfiguration<TKey, TEntity> configuration, string shardKey);

    public string GetDatabase(IMongoEntityDefaultConfiguration<TKey, TEntity> configuration, string shardKey);
}
