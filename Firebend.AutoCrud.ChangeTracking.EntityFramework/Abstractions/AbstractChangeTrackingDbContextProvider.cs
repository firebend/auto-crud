using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.DbContexts;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
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

            try
            {
                if (context.Database.GetService<IDatabaseCreator>() is RelationalDatabaseCreator dbCreator)
                {
                    await dbCreator
                        .CreateTablesAsync(cancellationToken)
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

            return context;
        }
    }
}
