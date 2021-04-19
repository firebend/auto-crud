using System;
using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface IEntityTransactionOutbox
    {
        Task AddEnrollmentAsync(Guid transactionId, IEntityTransactionOutboxEnrollment enrollment, CancellationToken cancellationToken);

        Task InvokeEnrollmentsAsync(Guid transactionId, CancellationToken cancellationToken);

        Task ClearEnrollmentsAsync(Guid transactionId, CancellationToken cancellationToken);
    }
}
