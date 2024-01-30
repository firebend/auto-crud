using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Core.Interfaces.Models;

public static class EntityTransactionExtensions
{
    public static Task AddFunctionEnrollmentAsync(this IEntityTransaction source,
        Func<CancellationToken, Task> func,
        CancellationToken cancellationToken)
        => source.Outbox.AddEnrollmentAsync(source.Id.ToString(), new FunctionTransactionOutboxEnrollment(func), cancellationToken);
}
