using System;

namespace Firebend.AutoCrud.Web.Sample.DbContexts;

#pragma warning disable IDE0060
public static class DbFunctionsExtensions
{
    public static bool JsonArrayIsEmpty(string jsonColumn, string arrayPath)
    {
        throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
    }
    public static string JsonValue(string jsonColumn, string arrayPath)
    {
        throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
    }
}

#pragma warning restore IDE0060
