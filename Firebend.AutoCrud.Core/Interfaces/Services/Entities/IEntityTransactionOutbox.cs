using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities;

public interface IEntityTransactionOutbox
{
    public Task AddEnrollmentAsync(EntityTransactionOutboxEnrollment enrollment, CancellationToken cancellationToken);

    public Task InvokeEnrollmentsAsync(string transactionId, CancellationToken cancellationToken);

    public Task ClearEnrollmentsAsync(string transactionId, CancellationToken cancellationToken);

    public Task<Dictionary<string, List<EntityTransactionOutboxEnrollment>>> GetEnrollmentsAsync(CancellationToken cancellationToken);

    public Action<string, List<EntityTransactionOutboxEnrollment>> OnBeforeInvokeEnrollments { get; set; }
}
