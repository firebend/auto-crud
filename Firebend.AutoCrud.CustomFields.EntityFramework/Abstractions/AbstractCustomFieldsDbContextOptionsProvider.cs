using System;
using System.Data.Common;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions
{
    public abstract class AbstractCustomFieldsDbContextOptionsProvider<TKey, TEntity, TCustomFieldsEntity, TContext> :
        IDbContextOptionsProvider<Guid, TCustomFieldsEntity, TContext>
        where TCustomFieldsEntity : EfCustomFieldsModel<TKey, TEntity>, IEntity<Guid>
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TContext : DbContext, IDbContext
    {
        private readonly IDbContextOptionsProvider<TKey, TEntity, TContext> _optionsProvider;

        protected AbstractCustomFieldsDbContextOptionsProvider(IDbContextOptionsProvider<TKey, TEntity, TContext> optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        public DbContextOptions<TContext> GetDbContextOptions(string connectionString) => _optionsProvider.GetDbContextOptions(connectionString);
        public DbContextOptions<TContext> GetDbContextOptions(DbConnection connection) => _optionsProvider.GetDbContextOptions(connection);
    }
}
