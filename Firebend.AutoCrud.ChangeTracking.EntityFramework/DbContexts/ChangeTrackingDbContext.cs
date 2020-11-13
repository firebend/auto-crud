using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Comparers;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Converters;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.DbContexts
{
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

        public DbSet<ChangeTrackingEntity<TKey, TEntity>> Changes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChangeTrackingEntity<TKey, TEntity>>(changes =>
            {
                changes.ToTable($"{typeof(TEntity).Name}_Changes");
                changes.HasKey(x => x.Id);
                changes.Property(x => x.Action).HasMaxLength(25);
                changes.Property(x => x.Modified);
                changes.Property(x => x.Source).HasMaxLength(500);
                changes.Property(x => x.UserEmail).HasMaxLength(250);
                changes.Property(x => x.EntityId);
                MapJson(changes, x => x.Changes);
                MapJson(changes, x => x.Entity);
            });
        }

        private static void MapJson<TProperty>(EntityTypeBuilder<ChangeTrackingEntity<TKey, TEntity>> changes,
            Expression<Func<ChangeTrackingEntity<TKey, TEntity>, TProperty>> func) => changes.Property(func)
            .HasConversion(new EntityFrameworkJsonValueConverter<TProperty>())
            .Metadata
            .SetValueComparer(new EntityFrameworkJsonComparer<TProperty>());
    }
}
