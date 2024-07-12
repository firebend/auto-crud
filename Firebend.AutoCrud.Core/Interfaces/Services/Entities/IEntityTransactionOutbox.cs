using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities;

public interface IEntityTransactionOutbox
{
    Task AddEnrollmentAsync(EntityTransactionOutboxEnrollment enrollment, CancellationToken cancellationToken);

    Task InvokeEnrollmentsAsync(string transactionId, CancellationToken cancellationToken);

    Task ClearEnrollmentsAsync(string transactionId, CancellationToken cancellationToken);

    Task<Dictionary<string, List<EntityTransactionOutboxEnrollment>>> GetEnrollmentsAsync(CancellationToken cancellationToken);

    Action<string, List<EntityTransactionOutboxEnrollment>> OnBeforeInvokeEnrollments { get; set; }
}
