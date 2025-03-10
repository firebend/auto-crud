using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;

public interface IDomainEventPublisherService<in TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    public Task<TEntity> ReadAndPublishAddedEventAsync(TKey key,
        IEntityTransaction transaction,
        CancellationToken cancellationToken);

    public Task<TEntity> ReadAndPublishUpdateEventAsync(TKey key,
        TEntity previous,
        IEntityTransaction transaction,
        JsonPatchDocument<TEntity> patch,
        CancellationToken cancellationToken);

    public Task<TEntity> ReadAndPublishUpdateEventAsync(TKey key,
        TEntity previous,
        IEntityTransaction transaction,
        CancellationToken cancellationToken);

    public Task PublishDeleteEventAsync(TEntity entity,
        IEntityTransaction transaction,
        CancellationToken cancellationToken);
}
