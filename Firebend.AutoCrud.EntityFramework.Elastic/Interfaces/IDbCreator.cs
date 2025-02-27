using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

public interface IDbCreator
{
    public Task EnsureCreatedAsync(string rootConnectionString, string dbName, CancellationToken cancellationToken);
}
