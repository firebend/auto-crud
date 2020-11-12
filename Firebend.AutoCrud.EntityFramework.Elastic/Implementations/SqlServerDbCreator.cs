using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations
{
    public class SqlServerDbCreator : AbstractDbCreator
    {
        protected override string GetSqlCommand(string dbName) => $@"
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'{dbName}')
BEGIN
  CREATE DATABASE [{dbName}];
END;";

        public SqlServerDbCreator(ILogger<SqlServerDbCreator> logger) : base(logger)
        {
        }
    }
}
