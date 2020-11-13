using Firebend.AutoCrud.Core.Interfaces.Models;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Helpers
{
    public static class MongoIndexProviderHelpers
    {
        public static CreateIndexModel<T> FullText<T>(IndexKeysDefinitionBuilder<T> builder)
            => new CreateIndexModel<T>(builder.Text("$**"), new CreateIndexOptions { Name = "text" });

        public static CreateIndexModel<T> DateTimeOffset<T>(IndexKeysDefinitionBuilder<T> builder)
            where T : IModifiedEntity
            => new CreateIndexModel<T>(
            builder.Combine(builder.Ascending(x => x.CreatedDate), builder.Ascending(x => x.ModifiedDate)),
            new CreateIndexOptions { Name = "modified" });
    }
}
