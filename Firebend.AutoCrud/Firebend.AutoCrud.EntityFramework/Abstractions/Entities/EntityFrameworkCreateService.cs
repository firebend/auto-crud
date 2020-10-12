using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities
{
    public abstract class EntityFrameworkCreateService<TKey, TEntity> : IEntityCreateService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly AbstractDbContextRepo<TKey, TEntity> _contextRepo;

        protected EntityFrameworkCreateService(AbstractDbContextRepo<TKey, TEntity> contextRepo)
        {
            _contextRepo = contextRepo;
        }

        public Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return _contextRepo.AddAsync(entity, cancellationToken);
        }
    }
}