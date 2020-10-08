using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Threading
{
    public sealed class AsyncDuplicateLock
    {
        private sealed class RefCounted<T>
        {
            public RefCounted(T value)
            {
                RefCount = 1;
                Value = value;
            }

            public int RefCount { get; set; }
            
            public T Value { get; }
        }

        private static readonly Dictionary<object, RefCounted<SemaphoreSlim>> SemaphoreSlims
            = new Dictionary<object, RefCounted<SemaphoreSlim>>();

        private static SemaphoreSlim GetOrCreate(object key)
        {
            RefCounted<SemaphoreSlim> item;

            lock (SemaphoreSlims)
            {
                if (SemaphoreSlims.TryGetValue(key, out item))
                {
                    ++item.RefCount;
                }
                else
                {
                    item = new RefCounted<SemaphoreSlim>(new SemaphoreSlim(1, 1));
                    SemaphoreSlims[key] = item;
                }
            }
            
            return item.Value;
        }

        public IDisposable Lock(object key)
        {
            GetOrCreate(key).Wait();

            return new Releaser { Key = key };
        }

        public async Task<IDisposable> LockAsync(object key, CancellationToken cancellationToken = default, TimeSpan? timeout = null)
        {
            var didGetLock =  await GetOrCreate(key)
                .WaitAsync((int)(timeout?.TotalMilliseconds ?? -1), cancellationToken)
                .ConfigureAwait(false);

            return new Releaser { Key = key, DidGetLock = didGetLock};
        }

        private sealed class Releaser : IDisposable
        {
            public object Key { get; set; }
            
            public bool DidGetLock { get; set; }

            public void Dispose()
            {
                RefCounted<SemaphoreSlim> item;
                
                lock (SemaphoreSlims)
                {
                    item = SemaphoreSlims[Key];
                    --item.RefCount;

                    if (item.RefCount == 0)
                    {
                        SemaphoreSlims.Remove(Key);
                    }
                }

                if (DidGetLock)
                {
                    item.Value.Release();
                }
            }
        }
    }
}