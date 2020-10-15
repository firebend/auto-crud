using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Helpers
{
    public static class MongoIndexProviderHelpers
    {
        public static CreateIndexModel<T> FullText<T>(IndexKeysDefinitionBuilder<T> builder)
        {
            return new CreateIndexModel<T>(builder.Text("$**"), new CreateIndexOptions {Name = "text"});
        }
    }
}