using System;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Firebend.AutoCrud.Web.Abstractions
{
    public abstract class AbstractControllerWithKeyParser<TKey, TEntity, TVersion> : AbstractEntityControllerBase<TVersion>
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TVersion : class, IApiVersion
    {
        private readonly IEntityKeyParser<TKey, TEntity, TVersion> _keyParser;

        protected AbstractControllerWithKeyParser(IEntityKeyParser<TKey, TEntity, TVersion> keyParser,
            IOptions<ApiBehaviorOptions> apiOptions) : base(apiOptions)
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

        protected bool IsCustomFieldsEntity() => typeof(TEntity).IsAssignableToGenericType(typeof(ICustomFieldsEntity<>));

        protected bool HasCustomFieldsPopulated(object o)
        {
            var property = typeof(TEntity).GetProperty(nameof(ICustomFieldsEntity<Guid>.CustomFields));

            if (property == null)
            {
                return false;
            }

            var value = property.GetValue(o, null);

            return value != null;
        }
    }
}
