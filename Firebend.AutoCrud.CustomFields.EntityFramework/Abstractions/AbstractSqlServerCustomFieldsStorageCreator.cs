using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions
{
    internal static class SqlServerCustomFieldsStorageCreatorCache
    {
        public static readonly ConcurrentDictionary<string, Task<bool>> ExistsCache = new ConcurrentDictionary<string, Task<bool>>();
    }

    public class AbstractSqlServerCustomFieldsStorageCreator<TKey, TEntity> : BaseDisposable, ICustomFieldsStorageCreator<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        private readonly IDbContextProvider<TKey, TEntity> _contextProvider;
        private readonly IEntityTableCreator _tableCreator;

        public AbstractSqlServerCustomFieldsStorageCreator(IDbContextProvider<TKey, TEntity> contextProvider,
            IEntityTableCreator tableCreator)
        {
            _contextProvider = contextProvider;
            _tableCreator = tableCreator;
        }

        public async Task CreateIfNotExistsAsync(CancellationToken cancellationToken)
        {
            var key = await GetCacheKey(cancellationToken).ConfigureAwait(false);
            await SqlServerCustomFieldsStorageCreatorCache.ExistsCache.GetOrAdd(key, async _ =>
                {
                    var context = await _contextProvider
                        .GetDbContextAsync(cancellationToken)
                        .ConfigureAwait(false);

                    var created = await _tableCreator
                        .EnsureExistsAsync<CustomFieldsEntity<TKey, TEntity>>(context, cancellationToken)
                        .ConfigureAwait(false);

                    return created;
                })
                .ConfigureAwait(false);
        }

        protected virtual Task<string> GetCacheKey(CancellationToken cancellationToken) => Task.FromResult($"{typeof(TEntity).Name}");
    }
}
