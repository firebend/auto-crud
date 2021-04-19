using System;
using System.Data.Common;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions
{
    public abstract class AbstractCustomFieldsDbContextOptionsProvider<TKey, TEntity, TCustomFieldsEntity> : IDbContextOptionsProvider<Guid, TCustomFieldsEntity>
        where TCustomFieldsEntity : EfCustomFieldsModel<TKey, TEntity>, IEntity<Guid>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IDbContextOptionsProvider<TKey, TEntity> _optionsProvider;

        protected AbstractCustomFieldsDbContextOptionsProvider(IDbContextOptionsProvider<TKey, TEntity> optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        public DbContextOptions GetDbContextOptions(string connectionString) => _optionsProvider.GetDbContextOptions(connectionString);
        public DbContextOptions GetDbContextOptions(DbConnection connection) => _optionsProvider.GetDbContextOptions(connection);
    }
}
