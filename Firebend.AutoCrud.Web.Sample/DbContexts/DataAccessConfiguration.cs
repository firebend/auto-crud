using Microsoft.Extensions.Configuration;

namespace Firebend.AutoCrud.Web.Sample.DbContexts
{
    public static class DataAccessConfiguration
    {
        private static IConfiguration _configuration;

        public static IConfiguration GetConfiguration()
        {
            _configuration ??= new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddUserSecrets("Firebend.AutoCrud")
                .AddEnvironmentVariables()
                .Build();

            return _configuration;
        }
    }
}
