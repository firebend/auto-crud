using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Models;

namespace Firebend.AutoCrud.Web.Implementations.ViewModelMappers;

public class DefaultCreateUpdateViewModelMapper<TKey, TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    public Task<TEntity> FromAsync(DefaultCreateUpdateViewModel<TKey, TEntity> model, CancellationToken _ = default)
        => Task.FromResult(model.Body);

    public Task<IEnumerable<TEntity>> FromAsync(IEnumerable<DefaultCreateUpdateViewModel<TKey, TEntity>> model,
        CancellationToken _ = default)
        => Task.FromResult(model.Select(m => m.Body));

    public Task<DefaultCreateUpdateViewModel<TKey, TEntity>> ToAsync(TEntity entity, CancellationToken _ = default)
        => Task.FromResult(new DefaultCreateUpdateViewModel<TKey, TEntity> { Body = entity });

    public Task<IEnumerable<DefaultCreateUpdateViewModel<TKey, TEntity>>> ToAsync(IEnumerable<TEntity> entity,
        CancellationToken _ = default)
        => Task.FromResult(entity.Select(e => new DefaultCreateUpdateViewModel<TKey, TEntity> { Body = e }));
}
