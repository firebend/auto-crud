using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public class DefaultEntityValidationService<TKey, TEntity> : IEntityValidationService<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public Task<ModelStateResult<TEntity>> ValidateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            return Task.FromResult(ModelStateResult.Success(entity));
        }
    }
}