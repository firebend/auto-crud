using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.DbContexts;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Pooling;
using Firebend.AutoCrud.Core.Threading;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Abstractions
{
    public abstract class AbstractChangeTrackingDbContextProvider<TEntityKey, TEntity> :
        IChangeTrackingDbContextProvider<TEntityKey, TEntity>
        where TEntity : class, IEntity<TEntityKey>
        where TEntityKey : struct
    {
        private readonly ILogger _logger;
        private readonly IDbContextOptionsProvider<TEntityKey, TEntity> _optionsProvider;
        private readonly IDbContextConnectionStringProvider<TEntityKey, TEntity> _connectionStringProvider;

        public AbstractChangeTrackingDbContextProvider(ILogger<AbstractChangeTrackingDbContextProvider<TEntityKey, TEntity>> logger,
            IDbContextOptionsProvider<TEntityKey, TEntity> optionsProvider,
            IDbContextConnectionStringProvider<TEntityKey, TEntity> connectionStringProvider)
        {
            _logger = logger;
            _optionsProvider = optionsProvider;
            _connectionStringProvider = connectionStringProvider;
        }

        public async Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
        {
            var connectionString = await _connectionStringProvider
                .GetConnectionStringAsync(cancellationToken)
                .ConfigureAwait(false);

            var options = _optionsProvider.GetDbContextOptions(connectionString);
            var context = new ChangeTrackingDbContext<TEntityKey, TEntity>(options);

            var runKey = AutoCrudObjectPool.InterpolateString(
                nameof(ChangeTrackingEntity<TEntityKey, TEntity>),
                ".CreateTables.",
                typeof(TEntityKey).Name,
                "_",
                typeof(TEntity).Name);

            await Run.OnceAsync(runKey, async ct =>
                {
                    try
                    {
                        if (context.Database.GetService<IDatabaseCreator>() is RelationalDatabaseCreator dbCreator)
                        {
                            await dbCreator
                                .CreateTablesAsync(ct)
                                .ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.StartsWith("There is already an object named"))
                        {
                            _logger.LogError("Error creating change tracking tables for context", ex);
                        }
                    }
                }, cancellationToken)
                .ConfigureAwait(false);

            return context;
        }
    }
}
