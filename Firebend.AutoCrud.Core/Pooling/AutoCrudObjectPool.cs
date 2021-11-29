using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace Firebend.AutoCrud.Core.Pooling
{
    public static class AutoCrudObjectPool
    {
        public static ObjectPoolProvider Instance { get; } = new LeakTrackingObjectPoolProvider(new DefaultObjectPoolProvider());

        public static ObjectPool<StringBuilder> StringBuilder { get; } = Instance.CreateStringBuilderPool();

        public static string InterpolateString(params object[] objects)
        {
            var sb = StringBuilder.Get();

            try
            {
                foreach (var o in objects)
                {
                    sb.Append(o);
                }

                return sb.ToString();
            }
            finally
            {
                StringBuilder.Return(sb);
            }
        }
    }
}
