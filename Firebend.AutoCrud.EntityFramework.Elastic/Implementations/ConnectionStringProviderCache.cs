using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations;

internal static class ConnectionStringProviderCache
{
    public static readonly ConcurrentDictionary<string, Task<string>> ConnectionStringCache = new();
}
