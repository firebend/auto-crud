using Firebend.AutoCrud.Core.Interfaces.Models;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Helpers;

public static class MongoIndexProviderHelpers
{
    public static CreateIndexOptions WithCollation(this CreateIndexOptions options, string locale = null)
    {
        if (locale is null || options.Collation is not null)
        {
            return options;
        }

        options.Collation = new Collation(locale);

        return options;
    }
    public static CreateIndexModel<T> FullText<T>(IndexKeysDefinitionBuilder<T> builder)
        => new(builder.Text("$**"), new CreateIndexOptions { Name = "text" });

    public static CreateIndexModel<T> DateTimeOffset<T>(IndexKeysDefinitionBuilder<T> builder, string locale = null)
        where T : IModifiedEntity
        => new(
        builder.Combine(builder.Ascending(x => x.CreatedDate), builder.Ascending(x => x.ModifiedDate)),
        new CreateIndexOptions { Name = "modified" }.WithCollation(locale));
}
