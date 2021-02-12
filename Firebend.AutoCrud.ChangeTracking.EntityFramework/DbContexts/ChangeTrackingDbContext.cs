using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Comparers;
using Firebend.AutoCrud.EntityFramework.Converters;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.DbContexts
{
    /// <summary>
    /// Encapsulates logic for persisting entity changes using Entity Framework.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of key for the entity that is being tracked.
    /// </typeparam>
    /// <typeparam name="TEntity">
    /// The type of entity that is being tracked.
    /// </typeparam>
    public class ChangeTrackingDbContext<TKey, TEntity> : DbContext, IDbContext
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        public ChangeTrackingDbContext()
        {
        }

        public ChangeTrackingDbContext(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating the <see cref="DbSet{TEntity}"/> comprised of <see cref="ChangeTrackingEntity{TKey,TEntity}"/>.
        /// </summary>
        public DbSet<ChangeTrackingEntity<TKey, TEntity>> Changes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChangeTrackingEntity<TKey, TEntity>>(changes =>
            {
                var (table, schema) = GetTableName();

                changes.ToTable(table, schema);
                changes.HasKey(x => x.Id);
                changes.Property(x => x.Action).HasMaxLength(25);
                changes.Property(x => x.ModifiedDate);
                changes.Property(x => x.Source).HasMaxLength(500);
                changes.Property(x => x.UserEmail).HasMaxLength(250);
                changes.Property(x => x.EntityId);

                MapJson(changes, x => x.Changes);
                MapJson(changes, x => x.Entity);
            });
        }

        private static (string, string) GetTableName()
        {
            var entityType = typeof(TEntity);
            string tableName = null;

            if (typeof(IEntityName).IsAssignableFrom(entityType))
            {
                try
                {
                    var instance = Activator.CreateInstance(entityType);

                    if (instance is IEntityName entityName)
                    {
                        tableName = entityName.GetEntityName();
                    }
                }
                catch
                {
                    // ignored
                }
            }

            var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();
            var schema = tableAttribute?.Schema;

            if (string.IsNullOrWhiteSpace(tableName))
            {
                tableName = tableAttribute?.Name;
            }

            if (string.IsNullOrWhiteSpace(tableName))
            {
                tableName = entityType.Name;
            }

            tableName += "_Changes";

            return (tableName, schema);
        }

        private static void MapJson<TProperty>(EntityTypeBuilder<ChangeTrackingEntity<TKey, TEntity>> changes,
            Expression<Func<ChangeTrackingEntity<TKey, TEntity>, TProperty>> func) => changes.Property(func)
            .HasConversion(new EntityFrameworkJsonValueConverter<TProperty>())
            .Metadata
            .SetValueComparer(new EntityFrameworkJsonComparer<TProperty>());
    }
}
