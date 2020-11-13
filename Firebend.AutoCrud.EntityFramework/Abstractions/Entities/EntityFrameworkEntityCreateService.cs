using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities
{
    public abstract class EntityFrameworkEntityCreateService<TKey, TEntity> : IEntityCreateService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IEntityFrameworkCreateClient<TKey, TEntity> _createClient;

        protected EntityFrameworkEntityCreateService(IEntityFrameworkCreateClient<TKey, TEntity> createClient)
        {
            _createClient = createClient;
        }

        public virtual Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
            => _createClient.AddAsync(entity, cancellationToken);
    }
}
