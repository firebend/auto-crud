using System.Collections.Generic;
using System.Linq;
using Firebend.AutoCrud.Core.ObjectMapping;

namespace Firebend.AutoCrud.Core.Extensions;

public static class ObjectExtensions
{

    public static bool IsBetween<T>(this T item, T start, T end) => Comparer<T>.Default.Compare(item, start) >= 0
                                                                    && Comparer<T>.Default.Compare(item, end) <= 0;

    public static TU CopyPropertiesTo<T, TU>(this T source,
        TU dest,
        string[] propertiesToIgnore = null,
        string[] propertiesToInclude = null,
        bool includeObjects = true)
        => CopyPropertiesToObjectMapper(source, dest, propertiesToIgnore, propertiesToInclude, includeObjects);

    public static TU CopyPropertiesToObjectMapper<T, TU>(this T source,
        TU dest,
        string[] propertiesToIgnore = null,
        string[] propertiesToInclude = null,
        bool includeObjects = true,
        bool useMemoizer = true)
    {
        ObjectMapper.Copy(source, dest, propertiesToIgnore, propertiesToInclude, includeObjects, useMemoizer);
        return dest;
    }

    public static TU CopyPropertiesToReflection<T, TU>(this T source, TU dest,
        string[] propertiesToIgnore = null,
        string[] propertiesToInclude = null,
        bool includeObjects = true)
    {
        var ctx = new ObjectMapperContext(typeof(T), typeof(TU), propertiesToIgnore, propertiesToInclude, includeObjects);
        var props = ObjectMapper.GetMatchingProperties(ctx);

        foreach (var map in props)
        {
            map.TargetProperty.SetValue(dest, map.SourceProperty.GetValue(source, null), null);
        }

        return dest;
    }
}
