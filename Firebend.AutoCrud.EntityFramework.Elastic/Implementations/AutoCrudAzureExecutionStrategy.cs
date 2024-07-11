using System;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations;

public class AutoCrudAzureExecutionStrategy : ExecutionStrategy
{
    private readonly IExecutionStrategy _strategy;

    public AutoCrudAzureExecutionStrategy(DbContext context, int maxRetryCount, TimeSpan maxRetryDelay) : base(context, maxRetryCount, maxRetryDelay)
    {
        var shouldUseUserTransaction = context is IDbContext { UseUserDefinedTransaction: true };

        if (shouldUseUserTransaction)
        {
            Console.WriteLine("******************************* USER TRAN *****************");
            _strategy = new NonRetryingExecutionStrategy(context);
        }
        else
        {
            _strategy = new SqlServerRetryingExecutionStrategy(context, maxRetryCount, maxRetryDelay, null);
        }
    }

    public AutoCrudAzureExecutionStrategy(IServiceProvider provider,
        ExecutionStrategyDependencies dependencies,
        int maxRetryCount,
        TimeSpan maxRetryDelay) : base(dependencies, maxRetryCount, maxRetryDelay)
    {
        var shouldUseUserTransaction = dependencies.CurrentContext.Context is IDbContext { UseUserDefinedTransaction: true };

        if (shouldUseUserTransaction)
        {
            Console.WriteLine("******************************* USER TRAN *****************");
            _strategy = new NonRetryingExecutionStrategy(dependencies);
        }
        else
        {
            _strategy = new SqlServerRetryingExecutionStrategy(dependencies, maxRetryCount, maxRetryDelay, null);
        }
    }

    protected override void OnFirstExecution()
    {
        this.ExceptionsEncountered.Clear();
    }

    protected override bool ShouldRetryOn(Exception exception)
    {
        if (_strategy is NonRetryingExecutionStrategy)
        {
            return false;
        }

#pragma warning disable EF1001
        return SqlServerTransientExceptionDetector.ShouldRetryOn(exception);
#pragma warning restore EF1001
    }
}
