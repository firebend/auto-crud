using System;
using System.Collections.Generic;
using System.Linq;

namespace Firebend.AutoCrud.Core.ObjectMapping
{
    public abstract class BaseObjectMapper
    {

        protected abstract string MapTypes(Type source, Type target, params string[] propertiesToIgnore);
        public abstract void Copy(object source, object target, params string[] propertiesToIgnore);

        /// <summary>
        /// This virtual function finds matching properties between given objects. It depends on their names, readability, and writability.
        /// </summary>
        /// <param name="sourceType">The source object's type</param>
        /// <param name="targetType">The target object's type</param>
        /// <returns>Returns a list of PropertyMap.</returns>
        protected virtual IEnumerable<PropertyMap> GetMatchingProperties
            (Type sourceType, Type targetType)
        {
            var sourceProperties = sourceType.GetProperties();
            var targetProperties = targetType.GetProperties();

            var properties = (sourceProperties
                    .SelectMany(s => targetProperties, (s, t) => new { s, t })
                .Where(@t1 => @t1.s.Name == @t1.t.Name && @t1.s.CanRead && @t1.t.CanWrite)
                .Select(@t1 => new PropertyMap { SourceProperty = @t1.s, TargetProperty = @t1.t }))
                .ToList();
            return properties;
        }

        /// <summary>
        /// This virtual function builds a key to identify the converter method. It uses object names and combines them with defined pattern.
        /// </summary>
        /// <param name="sourceType">The source object type</param>
        /// <param name="targetType">The target object type</param>
        /// <returns>Returns a key as string</returns>
        /// <exception cref="Exception">If there is no FullName property on sent objects, an exception will throw</exception>
        protected virtual string GetMapKey(Type sourceType, Type targetType, params string[] propertiesToIgnore)
        {
            var chunks = new List<string> { "ObjectMapper", sourceType.FullName, targetType.FullName };

            if (propertiesToIgnore != null)
            {
                chunks.AddRange(propertiesToIgnore);
            }

            return string.Join('_', chunks);
        }
    }
}
