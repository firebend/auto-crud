using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities;

public interface IEntityTransactionOutbox
{
    Task AddEnrollmentAsync(string transactionId, IEntityTransactionOutboxEnrollment enrollment, CancellationToken cancellationToken);

    Task InvokeEnrollmentsAsync(string transactionId, CancellationToken cancellationToken);

    Task ClearEnrollmentsAsync(string transactionId, CancellationToken cancellationToken);
}
