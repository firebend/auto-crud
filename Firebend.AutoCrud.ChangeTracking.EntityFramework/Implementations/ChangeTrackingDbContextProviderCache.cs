using System.Collections.Concurrent;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Implementations;

internal static class ChangeTrackingDbContextProviderCache
{
    public static readonly ConcurrentDictionary<string, bool> ScaffoldCache = new();
}
