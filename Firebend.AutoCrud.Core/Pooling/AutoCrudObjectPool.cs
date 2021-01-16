using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace Firebend.AutoCrud.Core.Pooling
{
    public static class AutoCrudObjectPool
    {
        private static readonly ObjectPoolProvider _provider = new LeakTrackingObjectPoolProvider(new DefaultObjectPoolProvider());

        public static ObjectPoolProvider Instance => _provider;

        private static readonly ObjectPool<StringBuilder> _stringBuilder = Instance.CreateStringBuilderPool();

        public static ObjectPool<StringBuilder> StringBuilder => _stringBuilder;
    }
}
