using System;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations
{
    public class ElasticPoolDbCreator : AbstractDbCreator
    {
        private ShardMapMangerConfiguration _shardMapMangerConfiguration;

        public ElasticPoolDbCreator(ShardMapMangerConfiguration shardMapMangerConfiguration)
        {
            _shardMapMangerConfiguration = shardMapMangerConfiguration;

            if (string.IsNullOrWhiteSpace(_shardMapMangerConfiguration?.ElasticPoolName))
            {
                throw new ArgumentException("No elastic pool name provided.", nameof(shardMapMangerConfiguration));
            }
        }

        protected override string GetSqlCommand(string dbName) => $@"
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'{dbName}')
BEGIN
  CREATE DATABASE [{dbName}] ( SERVICE_OBJECTIVE = ELASTIC_POOL ( name = ""{_shardMapMangerConfiguration.ElasticPoolName}"" ) );
END;";
    }
}