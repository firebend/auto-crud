using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Elastic
{
    public class SqlServerElasticDbShardModel : ElasticDbShardModel
    {
        private string _dbName;

        public SqlServerElasticDbShardModel(string dbName)
        {
            _dbName = dbName;
        }

        public override DbConnection OpenConnectionForKey(string key, string connectionString)
        {
            EnsureCreated(_dbName, connectionString);
            
            return OpenConnection(_dbName, connectionString);
        }

        private static DbConnection OpenConnection(string dbName, string connectionString)
        {
            var connBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = dbName
            };

            var cString = connBuilder.ConnectionString;

            var conn = new SqlConnection(cString);
            conn.Open();
            return conn;
        }

        private static void EnsureCreated(string dbName, string connectionString)
        {
            using var conn = new SqlConnection(connectionString);
            
            conn.Open();

            using var command = conn.CreateCommand();
            command.CommandText = $@"
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'{dbName}')
BEGIN
  CREATE DATABASE [{dbName}];
END;";
            command.ExecuteNonQuery();
        }
    }
}