using System;
using System.Linq;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firebend.AutoCrud.CustomFields.EntityFramework
{
    public class CustomFieldEntityTypeConfiguration<TKey, TEntity> : IEntityTypeConfiguration<EfCustomFieldsModel<TKey, TEntity>>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        private readonly string _tableName;
        private readonly string _schema;

        public CustomFieldEntityTypeConfiguration(string tableName, string schema)
        {
            _tableName = tableName;
            _schema = schema;
        }
        public void Configure(EntityTypeBuilder<EfCustomFieldsModel<TKey, TEntity>> builder)
        {
            builder.ToTable(_tableName, _schema);
            builder.Property(x => x.Key).IsRequired().HasMaxLength(250);
            builder.Property(x => x.Value).IsRequired().HasMaxLength(250);
            builder.HasOne(x => x.Entity).WithMany(nameof(ICustomFieldsEntity<TKey>.CustomFields));
            builder.HasIndex(x => x.EntityId).IsClustered();
        }
    }

    public static class CustomFieldsTypeConfigurationExtensions
    {
        public static void AddCustomFieldsConfigurations(this IDbContext context, ModelBuilder builder)
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

            var customFieldsEntityType = typeof(EfCustomFieldsModel<,>);
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

                var tableName = $"{builder.Model.GetTableName(entityType)}_CustomFields";
                var schemaName = builder.Model.GetSchemaName(entityType);

                var instance = Activator.CreateInstance(configurationType, tableName, schemaName);

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
