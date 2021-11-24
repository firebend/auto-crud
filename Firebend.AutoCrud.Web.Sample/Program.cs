using Firebend.AutoCrud.Web.Sample;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureHostConfiguration(c => c.AddUserSecrets("Firebend.AutoCrud"));

Startup.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();
Startup.Configure(app, app.Environment);

await app.RunAsync();
