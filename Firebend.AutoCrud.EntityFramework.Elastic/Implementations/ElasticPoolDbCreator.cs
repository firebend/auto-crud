using System;
using Firebend.AutoCrud.EntityFramework.Elastic.Implementations.Abstractions;
using Firebend.AutoCrud.EntityFramework.Elastic.Models;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations;

public class ElasticPoolDbCreator : AbstractDbCreator
{
    private readonly ShardMapMangerConfiguration _shardMapMangerConfiguration;

    public ElasticPoolDbCreator(ILogger logger, ShardMapMangerConfiguration shardMapMangerConfiguration)
        : base(logger)
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
