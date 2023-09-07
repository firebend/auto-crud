using Firebend.AutoCrud.EntityFramework.CustomCommands.Json;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.CustomCommands;

public static class JsonModelBuilderExtensions
{

    public static ModelBuilder AddJsonFunctions(this ModelBuilder modelBuilder)
        => modelBuilder.AddJsonValueSupport()
            .AddJsonArrayIsEmptySupport()
            .AddJsonQuerySupport()
            .AddIsJonSupport()
            .AddJsonQuerySupport()
            .AddJsonPathExistsSupport();
}
