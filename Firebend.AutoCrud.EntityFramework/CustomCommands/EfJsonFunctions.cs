using System;
// ReSharper disable UnusedParameter.Global

namespace Firebend.AutoCrud.EntityFramework.CustomCommands;

#pragma warning disable IDE0060
public class EfJsonFunctions
{
    public static bool JsonArrayIsEmpty(string jsonColumn, string path) =>
        throw new InvalidOperationException(
            "This method is for use with Entity Framework Core only and has no in-memory implementation.");

    public static string JsonValue(string jsonColumn, string path) =>
        throw new InvalidOperationException(
            "This method is for use with Entity Framework Core only and has no in-memory implementation.");

    public static string JsonQuery(string jsonColumn, string path) =>
        throw new InvalidOperationException(
            "This method is for use with Entity Framework Core only and has no in-memory implementation.");

    public static int? IsJson(string jsonColumn) =>
        throw new InvalidOperationException(
            "This method is for use with Entity Framework Core only and has no in-memory implementation.");

    public static int? JsonPathExists(string jsonColumn, string path) =>
        throw new InvalidOperationException(
            "This method is for use with Entity Framework Core only and has no in-memory implementation.");
}

#pragma warning restore IDE0060
