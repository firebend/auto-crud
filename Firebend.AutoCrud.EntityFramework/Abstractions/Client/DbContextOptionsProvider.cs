using System;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public class DbContextOptionsProvider<TKey, TEntity> : IDbContextOptionsProvider<TKey, TEntity>
    {
        private readonly Func<string, DbContextOptions> _optionsFunc;

        public DbContextOptionsProvider(Func<string, DbContextOptions> optionsFunc)
        {
            _optionsFunc = optionsFunc;
        }

        public DbContextOptions GetDbContextOptions(string connectionString) => _optionsFunc(connectionString);
    }
}
