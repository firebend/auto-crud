using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions
{
    public abstract class AbstractSqlServerCustomFieldsStorageCreator<TKey, TEntity, TEfModelType> : BaseDisposable, ICustomFieldsStorageCreator<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
        where TEfModelType : EfCustomFieldsModel<TKey, TEntity>
    {
        private readonly IDbContextProvider<TKey, TEntity> _contextProvider;
        private readonly IEntityTableCreator _tableCreator;
        private readonly IMemoizer<bool> _memoizer;

        protected AbstractSqlServerCustomFieldsStorageCreator(IDbContextProvider<TKey, TEntity> contextProvider,
            IEntityTableCreator tableCreator,
            IMemoizer<bool> memoizer)
        {
            _contextProvider = contextProvider;
            _tableCreator = tableCreator;
            _memoizer = memoizer;
        }

        public async Task CreateIfNotExistsAsync(CancellationToken cancellationToken)
        {
            var key = await GetCacheKey(cancellationToken).ConfigureAwait(false);

            var memoizeKey = $"{key}.Sql.CustomFields.Creation";

            await _memoizer.MemoizeAsync(memoizeKey, async () =>
            {
                var context = await _contextProvider
                    .GetDbContextAsync(cancellationToken)
                    .ConfigureAwait(false);

                var created = await _tableCreator
                    .EnsureExistsAsync<TEfModelType>(context, cancellationToken)
                    .ConfigureAwait(false);

                return created;
            }, cancellationToken);
        }

        protected virtual Task<string> GetCacheKey(CancellationToken cancellationToken) => Task.FromResult($"{typeof(TEntity).Name}");
    }
}
