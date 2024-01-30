using System.Collections.Concurrent;

namespace Firebend.AutoCrud.EntityFramework.Client;

internal static class AbstractDbContextProviderCache
{
    public static readonly ConcurrentDictionary<string, bool> InitCache = new();
}
