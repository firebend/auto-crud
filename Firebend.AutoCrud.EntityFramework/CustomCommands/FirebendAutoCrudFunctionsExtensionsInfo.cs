using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Firebend.AutoCrud.EntityFramework.CustomCommands
{
    public class FirebendAutoCrudFunctionsExtensionsInfo : DbContextOptionsExtensionInfo
    {
        private const string Name = "FirebendAutoCrudDbFunctions";

        public FirebendAutoCrudFunctionsExtensionsInfo(IDbContextOptionsExtension instance) : base(instance)
        {
        }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => Name;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is FirebendAutoCrudFunctionsExtensionsInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
        }

        public override int GetServiceProviderHashCode() => Name.GetHashCode();
    }
}
