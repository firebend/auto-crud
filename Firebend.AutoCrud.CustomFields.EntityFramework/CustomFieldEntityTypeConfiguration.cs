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
    public abstract class AbstractCustomFieldEntityTypeConfiguration<TKey, TEntity, TEfModel> : IEntityTypeConfiguration<TEfModel>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
        where TEfModel : EfCustomFieldsModel<TKey, TEntity>
    {
        private readonly string _tableName;
        private readonly string _schema;

        protected AbstractCustomFieldEntityTypeConfiguration(string tableName, string schema)
        {
            _tableName = tableName;
            _schema = schema;
        }
        public virtual void Configure(EntityTypeBuilder<TEfModel> builder)
        {
            builder.ToTable(_tableName, _schema);
            builder.Property(x => x.Key).IsRequired().HasMaxLength(250);
            builder.Property(x => x.Value).IsRequired().HasMaxLength(250);
            builder.HasOne(x => x.Entity).WithMany(nameof(ICustomFieldsEntity<TKey>.CustomFields));
            builder.HasIndex(x => x.EntityId).IsClustered();
        }
    }

    public class CustomFieldEntityTenantTypeConfiguration<TKey, TEntity, TTenantKey> :
        AbstractCustomFieldEntityTypeConfiguration<TKey, TEntity, EfCustomFieldsModelTenant<TKey, TEntity, TTenantKey>>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
        where TTenantKey : struct
    {
        public CustomFieldEntityTenantTypeConfiguration(string tableName, string schema) : base(tableName, schema)
        {
        }

        public override void Configure(EntityTypeBuilder<EfCustomFieldsModelTenant<TKey, TEntity, TTenantKey>> builder)
        {
            base.Configure(builder);
            builder.Property(x => x.TenantId).IsRequired();
        }
    }

    public class CustomFieldEntityTypeConfiguration<TKey, TEntity> :
        AbstractCustomFieldEntityTypeConfiguration<TKey, TEntity, EfCustomFieldsModel<TKey, TEntity>>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        public CustomFieldEntityTypeConfiguration(string tableName, string schema) : base(tableName, schema)
        {
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

                var isTenantEntity = entityType.IsAssignableToGenericType(typeof(ITenantEntity<>));
                Type tenantKeyType = null;

                if (isTenantEntity)
                {
                    tenantKeyType = entityType.GetInterfaces()
                        .Where(x => x.IsGenericType)
                        .FirstOrDefault(x => x.IsAssignableToGenericType(typeof(ITenantEntity<>)))
                        ?.GetGenericArguments()
                        .FirstOrDefault();

                    if (tenantKeyType == null)
                    {
                        throw new Exception("Could not determine tenant key type for entity " + entityType.Name);
                    }
                }

                var customFieldsEntityType = isTenantEntity ?
                    typeof(EfCustomFieldsModelTenant<,,>).MakeGenericType(entityKey, entityType, tenantKeyType) :
                    typeof(EfCustomFieldsModel<,>).MakeGenericType(entityKey, entityType);

                var configType = isTenantEntity ? typeof(CustomFieldEntityTenantTypeConfiguration<,,>).MakeGenericType(entityKey, entityType, tenantKeyType) :
                    typeof(CustomFieldEntityTypeConfiguration<,>).MakeGenericType(entityKey, entityType);

                var tableName = $"{builder.Model.GetTableName(entityType)}_CustomFields";
                var schemaName = builder.Model.GetSchemaName(entityType);

                var instance = Activator.CreateInstance(configType, tableName, schemaName);

                var configureMethod = configType.GetMethod("Configure");

                var entityMethodGeneric = entityMethod.MakeGenericMethod(customFieldsEntityType);

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
