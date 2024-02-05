using Firebend.AutoCrud.EntityFramework.Comparers;
using Firebend.AutoCrud.EntityFramework.Converters;
using Firebend.AutoCrud.EntityFramework.CustomCommands;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.Tests.Ef;

public class TestContext : DbContext
{
    public DbSet<TestEntity> TestEntities { get; set; }

    public TestContext(DbContextOptions<TestContext> opt) : base(opt)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddJsonFunctions();
        var builder = modelBuilder.Entity<TestEntity>();

        var settings = JsonPatch.JsonSerializationSettings.DefaultJsonSerializationSettings.Configure(new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        });

        builder.Property(e => e.Nested)
            .HasConversion(new EntityFrameworkJsonValueConverter<NestedClass>(settings))
            .Metadata
            .SetValueComparer(new EntityFrameworkJsonComparer<NestedClass>(settings));
    }
}
