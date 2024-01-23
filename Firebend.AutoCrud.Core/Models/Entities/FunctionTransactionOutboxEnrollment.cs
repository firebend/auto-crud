using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.Core.Models.Entities;

public class FunctionTransactionOutboxEnrollment : IEntityTransactionOutboxEnrollment
{
    private readonly Func<CancellationToken, Task> _func;

    public FunctionTransactionOutboxEnrollment(Func<CancellationToken, Task> func)
    {
        _func = func;
    }

    public Task ActAsync(CancellationToken cancellationToken) => _func(cancellationToken);
}
