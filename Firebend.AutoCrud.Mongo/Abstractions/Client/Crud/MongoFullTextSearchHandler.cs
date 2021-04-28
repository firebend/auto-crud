using System.Linq;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public class MongoFullTextSearchHandler<TKey, TEntity, TSearch> : DefaultEntitySearchHandler<TKey, TEntity, TSearch>
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TSearch : EntitySearchRequest
    {
        public MongoFullTextSearchHandler() : base((queryable, search) =>
        {
            if (string.IsNullOrWhiteSpace(search?.Search))
            {
                return queryable;
            }

            var b = Builders<TEntity>.Filter.Text(search.Search);

            return queryable.Where(_ => b.Inject());
        })
        {
        }
    }
}
