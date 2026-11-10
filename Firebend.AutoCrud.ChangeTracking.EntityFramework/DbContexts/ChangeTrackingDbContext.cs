using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Abstractions;
using Firebend.AutoCrud.EntityFramework.Comparers;
using Firebend.AutoCrud.EntityFramework.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.DbContexts;

/// <summary>
/// Encapsulates logic for persisting entity changes using Entity Framework.
/// </summary>
/// <typeparam name="TKey">
/// The type of key for the entity that is being tracked.
/// </typeparam>
/// <typeparam name="TEntity">
/// The type of entity that is being tracked.
/// </typeparam>
/// <typeparam name="TChangeTrackingEntity">
/// The type of row persisted for each change. Defaults to <see cref="ChangeTrackingEntity{TKey,TEntity}"/>
/// for every existing caller; a consumer may supply its own subclass to persist extra columns.
/// </typeparam>
public class ChangeTrackingDbContext<TKey, TEntity, TChangeTrackingEntity>(
    DbContextOptions<ChangeTrackingDbContext<TKey, TEntity, TChangeTrackingEntity>> options,
    IChangeTrackingTableNameProvider<TKey, TEntity> tableNameProvider)
    : AbstractDbContext(options)
    where TKey : struct
    where TEntity : class, IEntity<TKey>
    where TChangeTrackingEntity : ChangeTrackingEntity<TKey, TEntity>
{
    /// <summary>
    /// Gets or sets a value indicating the <see cref="DbSet{TEntity}"/> comprised of <typeparamref name="TChangeTrackingEntity"/>.
    /// </summary>
    public DbSet<TChangeTrackingEntity> Changes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TChangeTrackingEntity>(changes =>
        {
            var (tableName, schema) = tableNameProvider.GetTableName();

            changes.ToTable(tableName, schema);
            changes.HasKey(x => x.Id);
            changes.Property(x => x.Action).HasMaxLength(25);
            changes.Property(x => x.ModifiedDate);
            changes.Property(x => x.Source).HasMaxLength(500);
            changes.Property(x => x.UserEmail).HasMaxLength(250);
            changes.Property(x => x.EntityId);

            // Reads filter and count on EntityId and page in ModifiedDate order; without this every
            // GET {id}/changes scans the whole table. The name is pinned so consumers that add the index
            // to existing tables out of band (CreateTables() never alters a table that already exists)
            // can use the same name and be skipped here, and vice versa.
            changes.HasIndex(x => new { x.EntityId, x.ModifiedDate })
                .HasDatabaseName($"IX_{tableName}_EntityId_ModifiedDate");

            MapJson(changes, x => x.Changes);
            MapJson(changes, x => x.Entity);

            changes.Ignore(x => x.DomainEventCustomContext);
        });
    }

    private static void MapJson<TProperty>(EntityTypeBuilder<TChangeTrackingEntity> changes,
        Expression<Func<TChangeTrackingEntity, TProperty>> func)
    {
        var settings =
            JsonPatch.JsonSerializationSettings.DefaultJsonSerializationSettings.Configure(
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

        changes.Property(func)
            .HasConversion(new EntityFrameworkJsonValueConverter<TProperty>(settings))
            .Metadata
            .SetValueComparer(new EntityFrameworkJsonComparer<TProperty>(settings));
    }
}
