using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities;

public interface IEntityValidationService<TKey, TEntity, TVersion>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
    where TVersion : class, IAutoCrudApiVersion
{
    Task<ModelStateResult<TEntity>> ValidateAsync(TEntity original,
        TEntity entity,
        JsonPatchDocument<TEntity> patch,
        CancellationToken cancellationToken);
}
