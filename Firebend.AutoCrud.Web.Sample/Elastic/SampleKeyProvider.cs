using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

namespace Firebend.AutoCrud.Web.Sample.Elastic
{
    public class SampleKeyProvider : IShardKeyProvider
    {
        public string GetShardKey() => "Firebend";
    }
}
