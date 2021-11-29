using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    internal static class DbContextProviderCaches
    {
        public static readonly ConcurrentDictionary<string, Task<bool>> InitCache = new();
    }

    public abstract class DbContextProvider<TKey, TEntity, TContext> : IDbContextProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TContext : DbContext, IDbContext
    {
        private readonly ILogger _logger;
        private readonly IDbContextConnectionStringProvider<TKey, TEntity> _connectionStringProvider;
        private readonly IDbContextOptionsProvider<TKey, TEntity> _optionsProvider;

        protected DbContextProvider(IDbContextConnectionStringProvider<TKey, TEntity> connectionStringProvider,
            IDbContextOptionsProvider<TKey, TEntity> optionsProvider,
            ILoggerFactory loggerFactory)
        {
            _connectionStringProvider = connectionStringProvider;
            _optionsProvider = optionsProvider;
            _logger = loggerFactory.CreateLogger<DbContextProvider<TKey, TEntity, TContext>>();
        }

        private async Task<IDbContext> CreateContextAsync(DbContextOptions<TContext> options, CancellationToken cancellationToken)
        {
            var factory = new PooledDbContextFactory<TContext>(options);
            var context = await factory.CreateDbContextAsync(cancellationToken);

            if (context is DbContext dbContext)
            {
                var contextType = typeof(TContext);
                await DbContextProviderCaches.InitCache.GetOrAdd(contextType.FullName ?? string.Empty, async _ =>
                {
                    try
                    {
                        await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to call ensure created");
                    }

                    try
                    {
                        await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Fail to call migrations");
                    }

                    return true;
                }).ConfigureAwait(false);
            }

            return context;
        }

        public async Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
        {
            var connectionString = await _connectionStringProvider
                .GetConnectionStringAsync(cancellationToken)
                .ConfigureAwait(false);

            var options = _optionsProvider.GetDbContextOptions<TContext>(connectionString);

            var context = await CreateContextAsync(options, cancellationToken);

            return context;
        }

        public async Task<IDbContext> GetDbContextAsync(DbConnection connection, CancellationToken cancellationToken = default)
        {
            var options = _optionsProvider.GetDbContextOptions<TContext>(connection);

            var context = await CreateContextAsync(options, cancellationToken);

            return context;
        }
    }
}
