using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Threading
{
    internal static class RunOnceCaches
    {
        private static readonly ConcurrentDictionary<string, bool> Cache = new ConcurrentDictionary<string, bool>();

        public static bool HasRan(string key)
        {
            if (Cache.ContainsKey(key))
            {
                return Cache[key];
            }

            return false;
        }

        public static void MarkAsRan(string key)
        {
            Cache.AddOrUpdate(key,
                true,
                (s, b) => true);

            var __ = "";
        }
    }
    
    public static class Run
    {
        public static async Task OnceAsync(string key, Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            if (RunOnceCaches.HasRan(key))
            {
                return;
            }
            
            using var _ = await new AsyncDuplicateLock().LockAsync(key, cancellationToken).ConfigureAwait(false);
            
            if (RunOnceCaches.HasRan(key))
            {
                return;
            }

            await action(cancellationToken).ConfigureAwait(false);
            
            RunOnceCaches.MarkAsRan(key);
        }
    }
}