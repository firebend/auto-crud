using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions;

public abstract class AbstractSqlServerCustomFieldsStorageCreator<TKey, TEntity, TEfModelType> : BaseDisposable, ICustomFieldsStorageCreator<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    where TEfModelType : EfCustomFieldsModel<TKey, TEntity>
{
    private readonly IDbContextProvider<TKey, TEntity> _contextProvider;
    private readonly IEntityTableCreator _tableCreator;
    private readonly IMemoizer _memoizer;
    private string _memoizeKey;

    protected AbstractSqlServerCustomFieldsStorageCreator(IDbContextProvider<TKey, TEntity> contextProvider,
        IEntityTableCreator tableCreator,
        IMemoizer memoizer)
    {
        _contextProvider = contextProvider;
        _tableCreator = tableCreator;
        _memoizer = memoizer;
    }

    public async Task CreateIfNotExistsAsync(CancellationToken cancellationToken)
    {
        var key = await GetCacheKey(cancellationToken).ConfigureAwait(false);

        _memoizeKey ??= $"{key}.Sql.CustomFields.Creation";

        await _memoizer.MemoizeAsync<bool, (IDbContextProvider<TKey, TEntity> dbContextProvider, IEntityTableCreator _tableCreator, CancellationToken cancellationToken)>(
            _memoizeKey, static async arg =>
        {
            var (dbContextProvider, tableCreator, cancellationToken) = arg;

            var context = await dbContextProvider
                .GetDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            var created = await tableCreator
                .EnsureExistsAsync<TEfModelType>(context, cancellationToken)
                .ConfigureAwait(false);

            return created;
        }, (_contextProvider, _tableCreator, cancellationToken), cancellationToken);
    }

    protected virtual Task<string> GetCacheKey(CancellationToken cancellationToken) => Task.FromResult(typeof(TEntity).Name);
}
