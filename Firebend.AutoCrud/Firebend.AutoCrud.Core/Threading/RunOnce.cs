using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Threading
{
    internal static class RunOnceCaches
    {
        private static readonly ConcurrentDictionary<string, object> Cache = new ConcurrentDictionary<string, object>();

        public static object GetValue(string key)
        {
            if (Cache.ContainsKey(key))
            {
                return Cache[key];
            }

            return default;
        }

        public static T GetValue<T>(string key)
        {
            var val = GetValue(key);

            if (val == null || val.Equals(default(T)))
            {
                return default(T);
            }

            return (T) val;
        }
        

        public static void UpdateValue(string key, object value)
        {
            Cache.AddOrUpdate(key,
                value,
                (s, b) => value);
        }
    }
    
    public static class Run
    {
        public static Task OnceAsync(string key, Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            return OnceAsync(key, async ct =>
            {
                await action(ct).ConfigureAwait(false);
                return true;
            }, cancellationToken);
        }
        
        public static async Task<T> OnceAsync<T>(string key, Func<CancellationToken, Task<T>> func, CancellationToken cancellationToken)
        {
            var temp = RunOnceCaches.GetValue<T>(key);

            if (temp != null && !temp.Equals(default(T)))
            {
                return temp;
            }
            
            using var _ = await new AsyncDuplicateLock().LockAsync(key, cancellationToken).ConfigureAwait(false);
            
            temp = RunOnceCaches.GetValue<T>(key);

            if (temp != null && !temp.Equals(default(T)))
            {
                return temp;
            }

            temp = await func(cancellationToken).ConfigureAwait(false);

            RunOnceCaches.UpdateValue(key, temp);

            return temp;
        }
    }
}