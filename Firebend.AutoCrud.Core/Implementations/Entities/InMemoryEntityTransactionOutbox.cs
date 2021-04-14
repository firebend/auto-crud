using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Threading;

namespace Firebend.AutoCrud.Core.Implementations.Entities
{
    public class InMemoryEntityTransactionOutbox : IEntityTransactionOutbox
    {
        private readonly Dictionary<Guid, List<IEntityTransactionOutboxEnrollment>> _enrollments = new();

        public async Task AddEnrollmentAsync(Guid transactionId, IEntityTransactionOutboxEnrollment enrollment, CancellationToken cancellationToken)
        {
            using var loc = await new AsyncDuplicateLock().LockAsync(transactionId, cancellationToken);
            _enrollments[transactionId] ??= new List<IEntityTransactionOutboxEnrollment>();
            _enrollments[transactionId].Add(enrollment);
        }

        public async Task InvokeEnrollmentsAsync(Guid transactionId, CancellationToken cancellationToken)
        {
            if (_enrollments.IsEmpty())
            {
                return;
            }

            using var loc = new AsyncDuplicateLock().LockAsync(transactionId, cancellationToken);

            var callbacks = _enrollments[transactionId];

            if (callbacks.IsEmpty())
            {
                return;
            }

            var tasks = callbacks.Select(async x =>
                {
                    try
                    {
                        await x.ActAsync(cancellationToken);
                        return null;
                    }
                    catch(Exception ex)
                    {
                        return ex;
                    }
                })
                .ToArray();

            await Task.WhenAll(tasks);
            _enrollments.Remove(transactionId);
        }

        public async Task ClearEnrollmentsAsync(Guid transactionId, CancellationToken cancellationToken)
        {
            using var loc = await new AsyncDuplicateLock().LockAsync(transactionId, cancellationToken);
            _enrollments.Remove(transactionId);
        }
    }
}
