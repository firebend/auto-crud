using Firebend.AutoCrud.CustomFields.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Comparers;
using Firebend.AutoCrud.EntityFramework.Converters;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firebend.AutoCrud.Web.Sample.DbContexts;

public class PersonDbContext : DbContext, IDbContext
{
    public PersonDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<EfPerson> People { get; set; }

    public DbSet<EfPet> Pets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        this.AddCustomFieldsConfigurations(modelBuilder);

        AddDataAuthConfigurations(modelBuilder);
        modelBuilder.AddJsonArrayIsEmptySupport();
        modelBuilder.AddJsonValueSupport();
    }

    private void AddDataAuthConfigurations(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.GetInterface(nameof(IEntityDataAuth)) != null)
            {
                var builder = new EntityTypeBuilder<IEntityDataAuth>(entityType);
                ConfigureDataAuthEntity(builder);
            }
        }
    }

    private static void ConfigureDataAuthEntity(EntityTypeBuilder<IEntityDataAuth> builder) =>
        builder.Property(e => e.DataAuth).HasConversion(new EntityFrameworkJsonValueConverter<DataAuth>())
            .Metadata
            .SetValueComparer(new EntityFrameworkJsonComparer<DataAuth>());
}
