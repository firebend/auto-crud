using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Firebend.AutoCrud.Web.Abstractions
{
    public abstract class AbstractControllerWithKeyParser<TKey, TEntity> : AbstractEntityControllerBase
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IEntityKeyParser<TKey, TEntity> _keyParser;
        private readonly IOptions<ApiBehaviorOptions> _apiOptions;

        protected AbstractControllerWithKeyParser(IEntityKeyParser<TKey, TEntity> keyParser,
            IOptions<ApiBehaviorOptions> apiOptions) : base(apiOptions)
        {
            _keyParser = keyParser;
            _apiOptions = apiOptions;
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
