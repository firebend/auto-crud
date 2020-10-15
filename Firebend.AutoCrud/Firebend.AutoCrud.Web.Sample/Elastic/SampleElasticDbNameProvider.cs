using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

namespace Firebend.AutoCrud.Web.Sample.Elastic
{
    public class SampleElasticDbNameProvider : IElasticShardDatabaseNameProvider
    {
        public string GetShardDatabaseName(string key) => $"{key}_Sample";
    }
}