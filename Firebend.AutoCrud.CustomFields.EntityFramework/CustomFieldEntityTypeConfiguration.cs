using System;
using System.Linq;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firebend.AutoCrud.CustomFields.EntityFramework
{
    public class EfCustomFieldsEntity<TKey, TEntity> : CustomFieldsEntity<TKey>
        where TKey : struct
    {
        public TEntity Entity { get; set; }
    }

    public class CustomFieldEntityTypeConfiguration<TKey, TEntity> : IEntityTypeConfiguration<EfCustomFieldsEntity<TKey, TEntity>>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        public void Configure(EntityTypeBuilder<EfCustomFieldsEntity<TKey, TEntity>> builder)
        {
            builder.Property(x => x.Key).IsRequired().HasMaxLength(250);
            builder.Property(x => x.Value).IsRequired().HasMaxLength(250);
            builder.HasOne<TEntity>().WithMany().HasForeignKey(x => x.EntityId);
            builder.HasIndex(x => x.EntityId).IsClustered();
        }
    }

    public static class CustomFieldsTypeConfigurationExtensions
    {
        public static void AddCustomFieldsConfigurations(this DbContext context, ModelBuilder builder)
        {
            var entityTypes = context
                .GetType()
                .GetProperties()
                .Where(x => x.PropertyType.IsAssignableToGenericType(typeof(DbSet<>)))
                .Where(x =>
                {
                    var args = x.PropertyType.GetGenericArguments().First();
                    var isCustomFieldEntity = args.IsAssignableToGenericType(typeof(ICustomFieldsEntity<>)) &&
                                              args.IsAssignableToGenericType(typeof(IEntity<>));
                    return isCustomFieldEntity;
                })
                .Select(x => x.PropertyType.GetGenericArguments().First())
                .ToList();

            var entityMethod = builder
                .GetType()
                .GetMethods()
                .FirstOrDefault(x => x.Name == nameof(ModelBuilder.Entity) && x.IsGenericMethod);

            var customFieldsEntityType = typeof(EfCustomFieldsEntity<,>);
            var configType = typeof(CustomFieldEntityTypeConfiguration<,>);

            foreach (var entityType in entityTypes)
            {
                var entityKey = entityType.GetInterfaces()
                    .Where(x => x.IsGenericType)
                    .FirstOrDefault(x => x.IsAssignableToGenericType(typeof(IEntity<>)))
                    ?.GetGenericArguments()
                    .FirstOrDefault();

                if (entityKey == null)
                {
                    continue;
                }

                var customFieldsType = customFieldsEntityType.MakeGenericType(entityKey, entityType);
                var configurationType = configType.MakeGenericType(entityKey, entityType);

                var instance = Activator.CreateInstance(configurationType);
                var configureMethod = configurationType.GetMethod("Configure");

                var entityMethodGeneric = entityMethod.MakeGenericMethod(customFieldsType);
                var entityResult = entityMethodGeneric.Invoke(builder, new object[]
                {
                });
                configureMethod.Invoke(instance, new[]
                {
                    entityResult
                });
            }
        }
    }
}
