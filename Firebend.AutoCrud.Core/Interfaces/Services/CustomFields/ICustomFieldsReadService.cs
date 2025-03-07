using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;

public interface ICustomFieldsReadService<TKey, TEntity> : IDisposable
    where TKey : struct
    where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
{
    public Task<CustomFieldsEntity<TKey>> GetByKeyAsync(TKey entityId, TKey key,
        CancellationToken cancellationToken);

    public Task<CustomFieldsEntity<TKey>> GetByKeyAsync(TKey entityId, TKey key,
        IEntityTransaction transaction,
        CancellationToken cancellationToken);

    public Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(TKey entityId,
        CancellationToken cancellationToken);

    public Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(TKey entityId,
        Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        CancellationToken cancellationToken);

    public Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(TKey entityId,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(TKey entityId,
        Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<bool> ExistsAsync(TKey entityId, Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        CancellationToken cancellationToken);

    public Task<bool> ExistsAsync(TKey entityId, Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        IEntityTransaction transaction,
        CancellationToken cancellationToken);

    public Task<CustomFieldsEntity<TKey>> FindFirstOrDefaultAsync(TKey entityId,
        Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        CancellationToken cancellationToken);

    public Task<CustomFieldsEntity<TKey>> FindFirstOrDefaultAsync(TKey entityId,
        Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);
}
