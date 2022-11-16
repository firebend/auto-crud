using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

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
            builder.Property(x => x.Id).HasAnnotation("SqlServer:Clustered", false);

            builder.Property(x => x.Key).IsRequired().HasMaxLength(250);
            builder.Property(x => x.Value).IsRequired().HasMaxLength(250);
            builder.HasIndex(x => x.EntityId).IsClustered();

            builder.HasOne(x => x.Entity)
                .WithMany(nameof(ICustomFieldsEntity<TKey>.CustomFields))
                .HasForeignKey(x => x.EntityId);
        }
    }
}
