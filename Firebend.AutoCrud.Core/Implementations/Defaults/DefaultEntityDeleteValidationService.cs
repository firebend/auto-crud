using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public class
        DefaultEntityDeleteValidationService<TKey, TEntity, TVersion> : IEntityDeleteValidationService<TKey, TEntity,
        TVersion>
        where TVersion : class, IAutoCrudApiVersion
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        public Task<ModelStateResult<TEntity>> ValidateAsync(TEntity entity, CancellationToken cancellationToken) =>
            Task.FromResult(ModelStateResult.Success(entity));
    }
}
