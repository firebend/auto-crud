using Firebend.AutoCrud.Core.Interfaces.Models;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Helpers
{
    public static class MongoIndexProviderHelpers
    {
        public static CreateIndexModel<T> FullText<T>(IndexKeysDefinitionBuilder<T> builder)
        {
            return new CreateIndexModel<T>(builder.Text("$**"), new CreateIndexOptions {Name = "text"});
        }
        
        public static CreateIndexModel<T> DateTimeOffset<T>(IndexKeysDefinitionBuilder<T> builder) 
            where T : IModifiedEntity 
        {
            return new CreateIndexModel<T>(
                builder.Combine(builder.Ascending(x => x.CreatedDate), builder.Ascending(x => x.ModifiedDate)), 
                new CreateIndexOptions {Name = "modified"});
        }
    }
}