using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Ids;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Implementations;

public class CombGuidMongoCollectionKeyGenerator<TEntity> : IMongoCollectionKeyGenerator<Guid, TEntity>
    where TEntity : IEntity<Guid>
{
    public Task<Guid> GenerateKeyAsync(CancellationToken cancellationToken)
        => Task.FromResult(CombGuid.New());
}
