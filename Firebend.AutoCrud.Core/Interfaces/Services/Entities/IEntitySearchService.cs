using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities;

public interface IEntitySearchService<TKey, TEntity, in TSearch> : IDisposable
    where TKey : struct
    where TEntity : class, IEntity<TKey>
    where TSearch : IEntitySearchRequest
{
    public Task<List<TEntity>> SearchAsync(TSearch request, CancellationToken cancellationToken);

    public Task<List<TEntity>> SearchAsync(TSearch request, IEntityTransaction entityTransaction, CancellationToken cancellationToken);

    public Task<EntityPagedResponse<TEntity>> PageAsync(TSearch request, CancellationToken cancellationToken);

    public Task<EntityPagedResponse<TEntity>> PageAsync(TSearch request, IEntityTransaction entityTransaction, CancellationToken cancellationToken);
}
