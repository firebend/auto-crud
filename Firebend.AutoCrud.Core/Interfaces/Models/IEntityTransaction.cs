using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface IEntityTransaction : IDisposable
{
    public Guid Id { get; }

    public Task CompleteAsync(CancellationToken cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken);

    public IEntityTransactionOutbox Outbox { get; }

    public EntityTransactionState State { get; set; }

    public DateTimeOffset StartedDate { get; set; }
}
