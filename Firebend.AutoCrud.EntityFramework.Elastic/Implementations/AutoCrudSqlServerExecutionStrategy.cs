using System;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations;

public class AutoCrudSqlServerExecutionStrategy : ExecutionStrategy
{
    private readonly IExecutionStrategy _strategy;

    private static bool IsDbContextUsingUserTransactions(DbContext context)
        => context is IDbContext { UseUserDefinedTransaction: true };

    public AutoCrudSqlServerExecutionStrategy(DbContext context, int maxRetryCount, TimeSpan maxRetryDelay) : base(context, maxRetryCount, maxRetryDelay)
    {
        var shouldUseUserTransaction = IsDbContextUsingUserTransactions(context);

        if (shouldUseUserTransaction)
        {
            _strategy = new NonRetryingExecutionStrategy(context);
        }
        else
        {
            _strategy = new SqlServerRetryingExecutionStrategy(context, maxRetryCount, maxRetryDelay, null);
        }
    }


    public AutoCrudSqlServerExecutionStrategy(ExecutionStrategyDependencies dependencies,
        int maxRetryCount,
        TimeSpan maxRetryDelay) : base(dependencies, maxRetryCount, maxRetryDelay)
    {
        var shouldUseUserTransaction = IsDbContextUsingUserTransactions(dependencies.CurrentContext.Context);

        if (shouldUseUserTransaction)
        {
            _strategy = new NonRetryingExecutionStrategy(dependencies);
        }
        else
        {
            _strategy = new SqlServerRetryingExecutionStrategy(dependencies, maxRetryCount, maxRetryDelay, null);
        }
    }

    protected override void OnFirstExecution() => this.ExceptionsEncountered.Clear();

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
