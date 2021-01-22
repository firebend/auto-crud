using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.EntityFramework.CustomCommands
{
    public class FirebendDbContextOptionsExtension : IDbContextOptionsExtension
    {

        private DbContextOptionsExtensionInfo _info;

        public void ApplyServices(IServiceCollection services) => services
            .AddSingleton<IMethodCallTranslatorPlugin, FirebendMethodCallTranslatorPlugin>();

        public void Validate(IDbContextOptions options)
        {
        }

        public DbContextOptionsExtensionInfo Info => _info ??= new MyDbContextOptionsExtensionInfo(this);

        private sealed class MyDbContextOptionsExtensionInfo : DbContextOptionsExtensionInfo
        {
            public MyDbContextOptionsExtensionInfo(IDbContextOptionsExtension instance) : base(instance) { }

            public override bool IsDatabaseProvider => false;

            public override string LogFragment => "";

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {

            }

            public override long GetServiceProviderHashCode()
            {
                return 0;
            }
        }
    }
}
