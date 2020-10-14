using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public class DefaultEntityValidationService<TKey, TEntity> : IEntityValidationService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        public Task<ModelStateResult<TEntity>> ValidateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            return Task.FromResult(ModelStateResult.Success(entity));
        }

        public Task<ModelStateResult<TEntity>> ValidateAsync(TEntity original,
            TEntity entity,
            JsonPatchDocument<TEntity> patch,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(ModelStateResult.Success(entity));
        }
    }
}