using System.Collections.Generic;
using Firebend.AutoCrud.Core.ObjectMapping;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.Core.Extensions
{
    public static class ObjectExtensions
    {

        public static bool IsBetween<T>(this T item, T start, T end) => Comparer<T>.Default.Compare(item, start) >= 0
                                                                        && Comparer<T>.Default.Compare(item, end) <= 0;

        public static TU CopyPropertiesTo<T, TU>(this T source, TU dest, params string[] propertiesToIgnore)
        {
            ObjectMapper.Instance.Copy(source, dest, propertiesToIgnore);
            return dest;
        }

        public static TU CopyPropertiesToWithObjects<T, TU>(this T source, TU dest, params string[] propertiesToIgnore)
        {
            ObjectMapper.Instance.Copy(source, dest, propertiesToIgnore, includeObjects: true);
            return dest;
        }
    }
}
