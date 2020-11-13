using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Threading
{
    internal static class RunOnceCaches
    {
        private static readonly IDictionary<string, object> Cache = new Dictionary<string, object>();

        public static object GetValue(string key) => Cache.ContainsKey(key) ? Cache[key] : default;

        public static T GetValue<T>(string key)
        {
            var val = GetValue(key);

            if (val == null || val.Equals(default(T)))
            {
                return default;
            }

            return (T)val;
        }


        public static void UpdateValue(string key, object value) => Cache[key] = value;
    }

    public static class Run
    {
        public static Task OnceAsync(string key, Func<CancellationToken, Task> action, CancellationToken cancellationToken = default) => OnceAsync(key,
            async ct =>
            {
                await action(ct).ConfigureAwait(false);
                return true;
            }, cancellationToken);

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


        public static void Once(string key, Action action) => Once(key, () =>
        {
            action();
            return true;
        });

        public static T Once<T>(string key, Func<T> func)
        {
            var temp = RunOnceCaches.GetValue<T>(key);

            if (temp != null && !temp.Equals(default(T)))
            {
                return temp;
            }

            using var _ = new AsyncDuplicateLock().Lock(key);

            temp = RunOnceCaches.GetValue<T>(key);

            if (temp != null && !temp.Equals(default(T)))
            {
                return temp;
            }

            temp = func();

            RunOnceCaches.UpdateValue(key, temp);

            return temp;
        }
    }
}
