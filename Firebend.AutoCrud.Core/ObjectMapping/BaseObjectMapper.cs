using System;
using System.Collections.Generic;
using System.Linq;

namespace Firebend.AutoCrud.Core.ObjectMapping
{
    public abstract class BaseObjectMapper
    {
        private const string ObjectMapperConst = "ObjectMapper";

        protected abstract string MapTypes(Type source, Type target, string[] propertiesToIgnore, bool includeObjects);

        public abstract void Copy<TSource, TTarget>(TSource source, TTarget target, string[] propertiesToIgnore = null, bool includeObjects = true);

        /// <summary>
        ///     This virtual function finds matching properties between given objects. It depends on their names, readability, and writability.
        /// </summary>
        /// <param name="sourceType">The source object's type</param>
        /// <param name="targetType">The target object's type</param>
        /// <returns>Returns a list of PropertyMap.</returns>
        protected virtual IEnumerable<PropertyMap> GetMatchingProperties(Type sourceType, Type targetType, bool includeObjects)
        {
            var sourceProperties = sourceType.GetProperties();
            var targetProperties = targetType.GetProperties();

            var properties = sourceProperties.Join(
                targetProperties,
                x => x.Name,
                x => x.Name,
                (source, target) => new PropertyMap(source, target))
                .Where(x => x.SourceProperty.CanRead)
                .Where(x => x.TargetProperty.CanWrite)
                .Where(x => includeObjects
                            || x.SourceProperty.PropertyType.IsValueType
                            || x.SourceProperty.PropertyType == typeof(string))
                .Where(x => (x.SourceProperty.PropertyType.IsValueType
                            && x.SourceProperty.PropertyType.IsAssignableTo(x.TargetProperty.PropertyType))
                            || x.SourceProperty.PropertyType == x.TargetProperty.PropertyType)
                .ToArray();

            return properties;
        }

        /// <summary>
        ///     This virtual function builds a key to identify the converter method. It uses object names and combines them with defined pattern.
        /// </summary>
        /// <param name="sourceType">The source object type</param>
        /// <param name="targetType">The target object type</param>
        /// <returns>Returns a key as string</returns>
        /// <exception cref="Exception">If there is no FullName property on sent objects, an exception will throw</exception>
        protected virtual string GetMapKey(Type sourceType, Type targetType, string[] propertiesToIgnore, bool includeObjects)
        {
            if (propertiesToIgnore is null || propertiesToIgnore.Length <= 0)
            {
                return $"{ObjectMapperConst}_{sourceType.FullName}_{targetType.FullName}_includeObjects{includeObjects}";
            }

            return $"{ObjectMapperConst}_{sourceType.FullName}_{targetType.FullName}_{string.Join('_', propertiesToIgnore)}_includeObjects{includeObjects}";
        }
    }
}
