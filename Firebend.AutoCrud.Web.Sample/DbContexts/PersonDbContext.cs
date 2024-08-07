using Firebend.AutoCrud.CustomFields.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Abstractions;
using Firebend.AutoCrud.EntityFramework.Comparers;
using Firebend.AutoCrud.EntityFramework.Converters;
using Firebend.AutoCrud.EntityFramework.CustomCommands;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.Web.Sample.DbContexts;

public class PersonDbContext : AbstractDbContext
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
        modelBuilder.AddJsonFunctions();
    }

    private void AddDataAuthConfigurations(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.GetInterface(nameof(IEntityDataAuth)) != null)
            {
#pragma warning disable EF1001 // Internal EF Core API usage.
                var builder = new EntityTypeBuilder<IEntityDataAuth>(entityType);
                ConfigureDataAuthEntity(builder);
#pragma warning restore EF1001
            }
        }
    }

    private static void ConfigureDataAuthEntity(EntityTypeBuilder<IEntityDataAuth> builder)
    {
        var settings = JsonPatch.JsonSerializationSettings.DefaultJsonSerializationSettings.Configure(new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        });

        builder.Property(e => e.DataAuth)
            .HasConversion(new EntityFrameworkJsonValueConverter<DataAuth>(settings))
            .Metadata
            .SetValueComparer(new EntityFrameworkJsonComparer<DataAuth>(settings));
    }
}
