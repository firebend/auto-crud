using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public abstract class DefaultEntityValidationService<TKey, TEntity, TVersion> : IEntityValidationService<TKey, TEntity, TVersion>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
    {
        public Task<ModelStateResult<TEntity>> ValidateAsync(TEntity original,
            TEntity entity,
            JsonPatchDocument<TEntity> patch,
            CancellationToken cancellationToken)
            => Task.FromResult(ModelStateResult.Success(entity));
    }
}
