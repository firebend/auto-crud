using System;
using System.Data.Common;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework
{
    public class DbContextOptionsProvider<TKey, TEntity> : IDbContextOptionsProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly Func<string, DbContextOptions> _optionsFunc;
        private readonly Func<DbConnection, DbContextOptions> _optionsConnectionFunc;

        public DbContextOptionsProvider(Func<string, DbContextOptions> optionsFunc,
            Func<DbConnection, DbContextOptions> optionsConnectionFunc)
        {
            _optionsFunc = optionsFunc;
            _optionsConnectionFunc = optionsConnectionFunc;
        }

        public DbContextOptions GetDbContextOptions(string connectionString) => _optionsFunc(connectionString);
        public DbContextOptions GetDbContextOptions(DbConnection connection) => _optionsConnectionFunc(connection);
    }
}
