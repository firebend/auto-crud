using System;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public class DbContextOptionsProvider<TKey, TEntity> : IDbContextOptionsProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly Func<string, DbContextOptions> _optionsFunc;

        public DbContextOptionsProvider(Func<string, DbContextOptions> optionsFunc)
        {
            _optionsFunc = optionsFunc;
        }

        public DbContextOptions GetDbContextOptions(string connectionString) => _optionsFunc(connectionString);
    }
}
