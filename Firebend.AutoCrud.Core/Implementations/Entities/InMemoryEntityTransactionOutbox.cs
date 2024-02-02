using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.Core.Implementations.Entities;

public class InMemoryEntityTransactionOutbox : IEntityTransactionOutbox
{
    private readonly Dictionary<string, List<IEntityTransactionOutboxEnrollment>> _enrollments = new();

    public async Task AddEnrollmentAsync(string transactionId, IEntityTransactionOutboxEnrollment enrollment, CancellationToken cancellationToken)
    {
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

        var tasks = callbacks.Select(async x =>
            {
                try
                {
                    await x.ActAsync(cancellationToken);

                    return null;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            })
            .ToArray();

        await Task.WhenAll(tasks);

        _enrollments.Remove(transactionId);
    }

    public async Task ClearEnrollmentsAsync(string transactionId, CancellationToken cancellationToken)
    {
        using var loc = await InMemoryEntityTransactionOutboxStatics
            .Locker
            .LockAsync(transactionId, cancellationToken);

        _enrollments.Remove(transactionId);
    }
}
