using Firebend.AutoCrud.EntityFramework.Elastic.Implementations.Abstractions;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations;

public class SqlServerDbCreator : AbstractDbCreator
{
    public SqlServerDbCreator(ILogger<SqlServerDbCreator> logger) : base(logger)
    {
    }

    protected override string GetSqlCommand(string dbName) => $@"
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'{dbName}')
BEGIN
  CREATE DATABASE [{dbName}];
END;";
}
