using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

namespace Firebend.AutoCrud.Web.Sample.Elastic
{
    public class SampleElasticKeyProvider : IElasticShardKeyProvider
    {
        public string GetShardKey() => "Firebend";
    }
}