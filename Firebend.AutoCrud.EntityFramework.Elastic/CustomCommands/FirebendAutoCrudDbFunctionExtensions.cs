using System;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Elastic.CustomCommands
{
    public static class FirebendAutoCrudDbFunctionExtensions
    {
        public static bool ContainsAny(this DbFunctions source, string property, string search) =>
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        public static bool ContainsAny(this DbFunctions source, string property, string search, int language) =>
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");

        public static bool FreeTextAny(this DbFunctions source,  string property, string search) =>
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");

        public static bool FreeTextAny(this DbFunctions source,  string property, string search, int language) =>
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
    }
}
