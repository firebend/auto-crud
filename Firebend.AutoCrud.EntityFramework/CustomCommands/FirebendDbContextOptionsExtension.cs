using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.EntityFramework.CustomCommands;

public class FirebendDbContextOptionsExtension : IDbContextOptionsExtension
{
    public void ApplyServices(IServiceCollection services)
    {
        _ = new EntityFrameworkRelationalServicesBuilder(services)
            .TryAdd<IMethodCallTranslatorPlugin, FirebendMethodCallTranslatorPlugin>();
    }

    public void Validate(IDbContextOptions options)
    {
    }

    public DbContextOptionsExtensionInfo Info => field ??= new FirebendAutoCrudFunctionsExtensionsInfo(this);
}
