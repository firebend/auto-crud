using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions
{
    public class AbstractSqlServerCustomFieldsStorageCreator<TKey, TEntity> : BaseDisposable, ICustomFieldsStorageCreator<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        private readonly IDbContextProvider<TKey, TEntity> _contextProvider;

        public AbstractSqlServerCustomFieldsStorageCreator(IDbContextProvider<TKey, TEntity> contextProvider)
        {
            _contextProvider = contextProvider;
        }

        public async Task CreateIfNotExistsAsync(CancellationToken cancellationToken)
        {
            var context = await _contextProvider.GetDbContextAsync(cancellationToken);
            context.Set<CustomFieldsEntity<TKey>>().
        }
    }
}
