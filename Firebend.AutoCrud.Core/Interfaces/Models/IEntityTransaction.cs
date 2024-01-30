using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface IEntityTransaction : IDisposable
{
    Guid Id { get; }

    Task CompleteAsync(CancellationToken cancellationToken);

    Task RollbackAsync(CancellationToken cancellationToken);

    IEntityTransactionOutbox Outbox { get; }

    public EntityTransactionState State { get; set; }

    public DateTimeOffset StartedDate { get; set; }
}
