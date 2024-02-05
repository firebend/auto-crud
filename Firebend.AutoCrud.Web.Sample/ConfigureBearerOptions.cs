using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Firebend.AutoCrud.Web.Sample;

public class ConfigureBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(JwtBearerOptions options)
    {
        InternalConfiguration(options);
    }

    public void Configure(string name, JwtBearerOptions options)
    {
        InternalConfiguration(options);
    }

    private static void InternalConfiguration(JwtBearerOptions options)
    {
        options.Events ??= new();

        options.Events.OnForbidden = _ => Task.CompletedTask;

        options.Events.OnTokenValidated = _ => Task.CompletedTask;

        options.Events.OnAuthenticationFailed = _ => Task.CompletedTask;
    }
}
