using System.Collections.Generic;
using Firebend.AutoCrud.Core.ObjectMapping;

namespace Firebend.AutoCrud.Core.Extensions;

public static class ObjectExtensions
{

    public static bool IsBetween<T>(this T item, T start, T end) => Comparer<T>.Default.Compare(item, start) >= 0
                                                                    && Comparer<T>.Default.Compare(item, end) <= 0;

    public static TU CopyPropertiesTo<T, TU>(this T source, TU dest, string[] propertiesToIgnore = null, string[] propertiesToInclude = null, bool includeObjects = true)
    {
        ObjectMapper.Instance.Copy(source, dest, propertiesToIgnore, propertiesToInclude, includeObjects);
        return dest;
    }
}
