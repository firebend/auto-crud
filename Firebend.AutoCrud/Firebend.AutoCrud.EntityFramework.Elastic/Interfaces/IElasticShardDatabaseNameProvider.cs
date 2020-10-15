namespace Firebend.AutoCrud.EntityFramework.Elastic.Interfaces
{
    public interface IElasticShardDatabaseNameProvider
    {
        string GetShardDatabaseName();
    }
}