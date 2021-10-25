using System;
using System.Collections.Generic;
using System.Linq;

namespace Firebend.AutoCrud.Core.ObjectMapping
{
    public abstract class BaseObjectMapper
    {

        protected abstract void MapTypes(Type source, Type target);
        public abstract void Copy(object source, object target);

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

            var properties = (from s in sourceProperties
                from t in targetProperties
                where s.Name == t.Name &&
                      s.CanRead &&
                      t.CanWrite
                select new PropertyMap
                {
                    SourceProperty = s,
                    TargetProperty = t
                }).ToList();
            return properties;
        }

        /// <summary>
        /// This virtual function builds a key to identify the converter method. It uses object names and combines them with defined pattern.
        /// </summary>
        /// <param name="sourceType">The source object type</param>
        /// <param name="targetType">The target object type</param>
        /// <returns>Returns a key as string</returns>
        /// <exception cref="Exception">If there is no FullName property on sent objects, an exception will throw</exception>
        protected virtual string GetMapKey(Type sourceType, Type targetType)
        {
            var keyName = "Copy_";

            if (sourceType.FullName != null)
            {
                keyName += sourceType.FullName.Replace(".", "_").Replace("+", "_");
                keyName += "_";
            }
            else
            {
                throw new Exception();
            }

            if (targetType.FullName != null)
            {
                keyName += targetType.FullName.Replace(".", "_").Replace("+", "_");
            }
            else
            {
                throw new Exception();
            }

            return keyName;
        }
    }
}
