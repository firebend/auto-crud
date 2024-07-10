using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Core.Interfaces.Models;

public static class EntityTransactionExtensions
{
    public static Task AddFunctionEnrollmentAsync<TEntity, TEnrollment>(this IEntityTransaction source,
        Func<CancellationToken, Task> func,
        CancellationToken cancellationToken)
    where TEntity : class
    where TEnrollment : FunctionTransactionOutboxEnrollment, new()
    {
        var handler = new TEnrollment();
        handler.SetFunc(func);
        var enrollment = new EntityTransactionOutboxEnrollment(source.Id.ToString(), handler, typeof(TEntity));

        return source.Outbox.AddEnrollmentAsync(enrollment, cancellationToken);
    }
}
