using Firebend.AutoCrud.EntityFramework.CustomCommands.Json;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.CustomCommands;

public static class JsonModelBuilderExtensions
{

    //********************************************
    // Author: JMA
    // Date: 2023-09-08 09:06:15
    // Comment: I've had issues getting this to work as method translator plugins like we do with
    // the contains any stuff. Apparently it works only in a query.Where()
    // however if you add query.Where(x => x.Something == true && ) then try to use the function it won't get called to translate
    // so we need to leave them as db function calls for the time being
    //*******************************************
    public static ModelBuilder AddJsonFunctions(this ModelBuilder modelBuilder)
        => modelBuilder.AddJsonValueSupport()
            .AddJsonArrayIsEmptySupport()
            .AddJsonQuerySupport()
            .AddIsJsonSupport()
            .AddJsonQuerySupport()
            .AddJsonPathExistsSupport();
}
