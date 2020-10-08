using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IConfigureCollection
    {
        Task ConfigureAsync(CancellationToken cancellationToken);
    }

    public interface IConfigureCollection<TEntity, TKey> : IConfigureCollection 
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        
    }
}