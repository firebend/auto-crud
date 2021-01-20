using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Pooling;
using Firebend.AutoCrud.Core.Threading;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public abstract class DbContextProvider<TKey, TEntity, TContext> : IDbContextProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TContext : class, IDbContext
    {
        private readonly IDbContextConnectionStringProvider<TKey, TEntity> _connectionStringProvider;
        private readonly IDbContextOptionsProvider<TKey, TEntity> _optionsProvider;

        protected DbContextProvider(IDbContextConnectionStringProvider<TKey, TEntity> connectionStringProvider,
            IDbContextOptionsProvider<TKey, TEntity> optionsProvider)
        {
            _connectionStringProvider = connectionStringProvider;
            _optionsProvider = optionsProvider;
        }

        public async Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
        {
            var connectionString = await _connectionStringProvider
                .GetConnectionStringAsync(cancellationToken)
                .ConfigureAwait(false);

            var options = _optionsProvider.GetDbContextOptions(connectionString);

            var contextType = typeof(TContext);
            var instance = Activator.CreateInstance(contextType, options);

            var context = instance as TContext;

            var shardEnsureCreatedKey = AutoCrudObjectPool.InterpolateString(nameof(DbContextProvider<TKey, TEntity, TContext>),
                ".",
                contextType.Name,
                ".Init");

            await Run.OnceAsync(shardEnsureCreatedKey, async ct =>
                {
                    await context.Database
                        .EnsureCreatedAsync(ct)
                        .ConfigureAwait(false);

                    await context.Database
                        .MigrateAsync(ct)
                        .ConfigureAwait(false);
                }, cancellationToken)
                .ConfigureAwait(false);

            return context;
        }

    }
}
