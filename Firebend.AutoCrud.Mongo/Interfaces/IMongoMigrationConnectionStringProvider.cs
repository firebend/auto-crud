namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoMigrationConnectionStringProvider
{
    public string GetConnectionString();
}
