using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Abstractions
{
    public abstract class AbstractControllerWithKeyParser<TKey, TEntity> : ControllerBase
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IEntityKeyParser<TKey, TEntity> _keyParser;

        protected AbstractControllerWithKeyParser(IEntityKeyParser<TKey, TEntity> keyParser)
        {
            _keyParser = keyParser;
        }

        protected TKey? GetKey(string key)
        {
            var id = _keyParser.ParseKey(key);

            if (!id.HasValue)
            {
                ModelState.AddModelError(nameof(id), "The id is not valid");
                return null;
            }

            if (id.Value.Equals(default(TKey)) || id.Equals(null))
            {
                ModelState.AddModelError(nameof(id), "An id is required");
                return null;
            }

            return id;
        }
    }
}
