namespace Firebend.AutoCrud.EntityFramework.Elastic.Interfaces
{
    public interface IElasticShardKeyProvider
    {
        string GetShardKey();
    }
}