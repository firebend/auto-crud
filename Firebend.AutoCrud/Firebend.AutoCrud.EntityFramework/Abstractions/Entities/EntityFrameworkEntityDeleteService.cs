using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities
{
    public class EntityFrameworkEntityDeleteService<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityDeleteService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IEntityFrameworkDeleteClient<TKey, TEntity> _deleteClient;
        
        public EntityFrameworkEntityDeleteService(IDbContext context, IEntityFrameworkDeleteClient<TKey, TEntity> deleteClient) : base(context)
        {
            _deleteClient = deleteClient;
        }

        public Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken = default)
        {
            return _deleteClient.DeleteAsync(key, cancellationToken);
        }
    }
}