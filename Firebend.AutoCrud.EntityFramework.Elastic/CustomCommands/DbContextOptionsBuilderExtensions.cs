using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Firebend.AutoCrud.EntityFramework.Elastic.CustomCommands
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static T AddFirebendFunctions<T>(this T  optionsBuilder)
            where T : IDbContextOptionsBuilderInfrastructure
        {
            optionsBuilder.AddOrUpdateExtension(new FirebendDbContextOptionsExtension());

            return optionsBuilder;
        }
    }
}
