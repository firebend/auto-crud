using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Threading
{
    internal static class RunOnceCaches
    {
        private static readonly IDictionary<string, object> Cache = new Dictionary<string, object>();

        public static object GetValue(string key)
        {
            return Cache.ContainsKey(key) ? Cache[key] : default;
        }

        public static T GetValue<T>(string key)
        {
            var val = GetValue(key);

            if (val == null || val.Equals(default(T)))
            {
                return default;
            }

            return (T) val;
        }
        

        public static void UpdateValue(string key, object value)
        {
            Cache[key] = value;
        }
    }
    
    public static class Run
    {
        public static Task OnceAsync(string key, Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            return OnceAsync(key, async ct =>
            {
                await action(ct).ConfigureAwait(false);
                return true;
            }, cancellationToken);
        }
        
        public static async Task<T> OnceAsync<T>(string key, Func<CancellationToken, Task<T>> func, CancellationToken cancellationToken = default)
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