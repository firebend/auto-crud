using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.JsonPatch;
using Firebend.AutoCrud.DomainEvents.MassTransit.Models.Messages;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit
{
    public class MassTransitDomainEventPublisher : IEntityDomainEventPublisher
    {
        private readonly IBus _bus;
        private readonly IJsonPatchDocumentGenerator _generator;

        public MassTransitDomainEventPublisher(IBus bus, IJsonPatchDocumentGenerator generator)
        {
            _bus = bus;
            _generator = generator;
        }

        public Task PublishEntityAddEventAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class
            => _bus.Publish(new EntityAddedDomainEvent<TEntity>
            {
                Entity = entity
            }, cancellationToken);

        public Task PublishEntityDeleteEventAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class
            => _bus.Publish(new EntityDeletedDomainEvent<TEntity>
            {
                Entity = entity
            }, cancellationToken);

        public Task PublishEntityUpdatedEventAsync<TEntity>(TEntity original, TEntity modified, CancellationToken cancellationToken = default)
            where TEntity : class
            => _bus.Publish(new EntityUpdatedDomainEvent<TEntity>
            {
                Previous = original,
                Patch = _generator.Generate(original, modified)
            }, cancellationToken);
    }
}