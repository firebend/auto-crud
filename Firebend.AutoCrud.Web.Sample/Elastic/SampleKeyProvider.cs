using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Web.Sample.Elastic
{
    public class SampleKeyProvider : IShardKeyProvider
    {
        public string GetShardKey() => "Firebend";
    }

    public class SampleKeyProviderMongo : IMongoShardKeyProvider
    {
        public string GetShardKey() => "Firebend";
    }
}
