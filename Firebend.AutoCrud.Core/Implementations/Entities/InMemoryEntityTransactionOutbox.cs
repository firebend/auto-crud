using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Core.Implementations.Entities;

public class InMemoryEntityTransactionOutbox : IEntityTransactionOutbox
{
    private readonly Dictionary<string, List<EntityTransactionOutboxEnrollment>> _enrollments = new();

    public async Task AddEnrollmentAsync(EntityTransactionOutboxEnrollment enrollment, CancellationToken cancellationToken)
    {
        var transactionId = enrollment.TransactionId;

        using var loc = await InMemoryEntityTransactionOutboxStatics
            .Locker
            .LockAsync(transactionId, cancellationToken);

        if (!_enrollments.ContainsKey(transactionId))
        {
            _enrollments[transactionId] = [enrollment];
            return;
        }

        _enrollments[transactionId] ??= [];
        _enrollments[transactionId].Add(enrollment);
    }

    public async Task InvokeEnrollmentsAsync(string transactionId, CancellationToken cancellationToken)
    {
        if (_enrollments.IsEmpty())
        {
            return;
        }

        using var loc = await InMemoryEntityTransactionOutboxStatics
            .Locker
            .LockAsync(transactionId, cancellationToken);

        if (!_enrollments.TryGetValue(transactionId, out var callbacks))
        {
            return;
        }

        if (callbacks.IsEmpty())
        {
            return;
        }

        OnBeforeInvokeEnrollments?.Invoke(transactionId, callbacks);

        var tasks = callbacks.Select(async x =>
            {
                try
                {
                    await x.Enrollment.ActAsync(cancellationToken);

                    return null;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            })
            .ToArray();

        var exceptions = (await Task.WhenAll(tasks)).Where(x => x is not null);

        if (exceptions.Any())
        {
            throw new AggregateException("One or more enrollment actions failed", exceptions);
        }

        _enrollments.Remove(transactionId);
    }

    public async Task ClearEnrollmentsAsync(string transactionId, CancellationToken cancellationToken)
    {
        using var loc = await InMemoryEntityTransactionOutboxStatics
            .Locker
            .LockAsync(transactionId, cancellationToken);

        _enrollments.Remove(transactionId);
    }

    public Task<Dictionary<string, List<EntityTransactionOutboxEnrollment>>> GetEnrollmentsAsync(
        CancellationToken cancellationToken)
        => Task.FromResult(_enrollments);

    public Action<string, List<EntityTransactionOutboxEnrollment>> OnBeforeInvokeEnrollments { get; set; }
}
