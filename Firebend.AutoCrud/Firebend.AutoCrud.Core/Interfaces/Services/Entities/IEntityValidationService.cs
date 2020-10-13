using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface IEntityValidationService<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        Task<ModelStateResult<TEntity>> ValidateAsync(TEntity entity, CancellationToken cancellationToken);
    }
}