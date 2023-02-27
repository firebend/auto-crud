using System;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Implementations.ViewModelMappers
{
    public class DefaultEntityKeyParser<TKey, TEntity, TVersion> : IEntityKeyParser<TKey, TEntity, TVersion>
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TVersion : class, IApiVersion
    {
        public TKey? ParseKey(string key)
        {
            var tKeyType = typeof(TKey);

            if (tKeyType == typeof(long))
            {
                if (long.TryParse(key, out var l))
                {
                    return (TKey)Convert.ChangeType(l, typeof(long));
                }
            }
            else if (tKeyType == typeof(int))
            {
                if (int.TryParse(key, out var i))
                {
                    return (TKey)Convert.ChangeType(i, typeof(int));
                }
            }
            else if (tKeyType == typeof(Guid))
            {
                if (Guid.TryParse(key, out var g))
                {
                    return (TKey)Convert.ChangeType(g, typeof(Guid));
                }
            }
            else if (tKeyType == typeof(string))
            {
                return (TKey)Convert.ChangeType(key, typeof(string));
            }

            return null;
        }
    }
}
