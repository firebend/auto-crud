using System.Data.Common;

namespace Firebend.AutoCrud.EntityFramework.Elastic
{
    public abstract class ElasticDbShardModel
    {
        public abstract DbConnection OpenConnectionForKey(string key, string connectionString);
    }
}