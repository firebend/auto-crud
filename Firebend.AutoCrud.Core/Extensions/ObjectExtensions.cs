using System.Collections.Generic;
using Firebend.AutoCrud.Core.ObjectMapping;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.Core.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        ///     Creates a deep copy of this object using json serialization
        /// </summary>
        /// <typeparam name="T">Captured implicitly</typeparam>
        /// <param name="source">The object to clone</param>
        /// <returns>A cloned object</returns>
        public static T Clone<T>(this T source)
            where T : class => ((object)source).Clone<T>();

        /// <summary>
        ///     Creates a deep copy cast of this object using json serialization
        /// </summary>
        /// <typeparam name="TOut">The type to deserialize as</typeparam>
        /// <param name="source">The object to clone</param>
        /// <returns>A cloned object of type TOut</returns>
        public static TOut Clone<TOut>(this object source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<TOut>(JsonConvert.SerializeObject(source));
        }

        public static bool IsBetween<T>(this T item, T start, T end) => Comparer<T>.Default.Compare(item, start) >= 0
                                                                        && Comparer<T>.Default.Compare(item, end) <= 0;

        public static TU CopyPropertiesTo<T, TU>(this T source, TU dest, params string[] propertiesToIgnore)
        {
            ObjectMapper.Instance.Copy(source, dest, propertiesToIgnore);
            return dest;
        }
    }
}
