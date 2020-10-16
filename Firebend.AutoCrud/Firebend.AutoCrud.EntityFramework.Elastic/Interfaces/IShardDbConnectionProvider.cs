using System.Data.Common;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Interfaces
{
    public interface IShardDbConnectionProvider
    {
        DbConnection GetConnection(string rootConnectionString, string key);
    }
}