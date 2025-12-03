using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    // ReSharper disable once UnusedTypeParameter
    public interface IEntityDeleteValidationService<TKey, TEntity, TVersion>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
    {
        public Task<ModelStateResult<TEntity>> ValidateAsync(TEntity entity, CancellationToken cancellationToken);
    }
}
