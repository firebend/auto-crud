using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

namespace Firebend.AutoCrud.Web.Sample.Elastic
{
    public class SampleDbNameProvider : IShardNameProvider
    {
        public string GetShardName(string key) => $"{key}_CrudSample";
    }
}
