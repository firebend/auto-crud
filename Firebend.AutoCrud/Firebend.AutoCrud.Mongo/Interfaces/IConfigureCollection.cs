using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IConfigureCollection
    {
        Task ConfigureAsync(CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedTypeParameter
    public interface IConfigureCollection<TKey, TEntity> : IConfigureCollection
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
    }
}