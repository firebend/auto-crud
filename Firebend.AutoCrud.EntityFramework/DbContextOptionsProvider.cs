using System;
using System.Data.Common;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework
{
    public class DbContextOptionsProvider<TKey, TEntity, TContext> : IDbContextOptionsProvider<TKey, TEntity, TContext>
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TContext : DbContext, IDbContext
    {
        private readonly Func<string, DbContextOptions<TContext>> _optionsFunc;
        private readonly Func<DbConnection, DbContextOptions<TContext>> _optionsConnectionFunc;

        public DbContextOptionsProvider(Func<string, DbContextOptions<TContext>> optionsFunc,
            Func<DbConnection, DbContextOptions<TContext>> optionsConnectionFunc)
        {
            _optionsFunc = optionsFunc;
            _optionsConnectionFunc = optionsConnectionFunc;
        }

        public DbContextOptions<TContext> GetDbContextOptions(string connectionString) => _optionsFunc(connectionString);
        public DbContextOptions<TContext> GetDbContextOptions(DbConnection connection) => _optionsConnectionFunc(connection);
        public DbContextOptions GetDbConnectionOptions(string connectionString) => null;

        public DbContextOptions GetDbConnectionOptions(DbConnection connection) => null;
    }
}
