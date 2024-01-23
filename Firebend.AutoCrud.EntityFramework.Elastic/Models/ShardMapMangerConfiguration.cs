namespace Firebend.AutoCrud.EntityFramework.Elastic.Models;

public class ShardMapMangerConfiguration
{
    public string Server { get; set; }

    public string ConnectionString { get; set; }

    public string MapName { get; set; }

    public string ShardMapManagerDbName { get; set; } = "Shards";

    public string ElasticPoolName { get; set; }
}
