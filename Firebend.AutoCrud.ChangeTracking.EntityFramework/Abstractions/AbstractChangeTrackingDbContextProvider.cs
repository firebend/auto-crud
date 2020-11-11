using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.DbContexts;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Threading;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.Data.SqlClient;
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
        private readonly IDbContextProvider<TEntityKey, TEntity> _contextProvider;
        private readonly ILogger _logger;

        public AbstractChangeTrackingDbContextProvider(IDbContextProvider<TEntityKey, TEntity> contextProvider,
            ILogger<AbstractChangeTrackingDbContextProvider<TEntityKey, TEntity>> logger)
        {
            _contextProvider = contextProvider;
            _logger = logger;
        }

        public async Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
        {
            var entityContext = await _contextProvider
                .GetDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            var db = entityContext as DbContext;
            var connectionString = db?.Database.GetDbConnection()?.ConnectionString;

            ChangeTrackingDbContext<TEntityKey, TEntity> context;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                context = new ChangeTrackingDbContext<TEntityKey, TEntity>();
            }
            else
            {
                var options = new DbContextOptionsBuilder()
                    .UseSqlServer(connectionString)
                    .Options;

                context = new ChangeTrackingDbContext<TEntityKey, TEntity>(options);
            }

            var runKey = $"{nameof(ChangeTrackingEntity<TEntityKey, TEntity>)}.CreateTables";

            await Run.OnceAsync(runKey, async ct =>
            {
                try
                {
                    if (context.Database.GetService<IDatabaseCreator>() is RelationalDatabaseCreator dbCreator)
                    {
                        await dbCreator
                            .CreateTablesAsync(cancellationToken)
                            .ConfigureAwait(false);
                    }
                }
                catch (SqlException)
                {

                }
                catch (Exception ex)
                {
                    _logger.LogError("Error creating change tracking tables for context", ex);
                }
            }, cancellationToken).ConfigureAwait(false);

            return context;
        }
    }
}
