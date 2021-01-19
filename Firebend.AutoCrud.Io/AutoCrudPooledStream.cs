using System.IO;
using Microsoft.IO;

namespace Firebend.AutoCrud.Io
{
    internal static class AutoCrudPooledStreamManager
    {
        public static readonly RecyclableMemoryStreamManager RecyclableMemoryStreamManager = new();
    }

    public static class AutoCrudPooledStream
    {
        public static MemoryStream GetStream(string name = null) => AutoCrudPooledStreamManager.RecyclableMemoryStreamManager.GetStream(name);
    }
}
