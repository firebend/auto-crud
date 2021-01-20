using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Connections
{
    public class DefaultDbContextConnectionStringProvider<TKey, TEntity> : IDbContextConnectionStringProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly string _connectionString;

        public DefaultDbContextConnectionStringProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Task<string> GetConnectionStringAsync(CancellationToken cancellationToken = default) => Task.FromResult(_connectionString);
    }
}
