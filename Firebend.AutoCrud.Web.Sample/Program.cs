using System;
using Firebend.AutoCrud.Web.Sample;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var processId = Environment.ProcessId;
Console.WriteLine($"Auto Crud Web Sample is running on process id {processId}");

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets("Firebend.AutoCrud");

Startup.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();
Startup.Configure(app, app.Environment, app.Services.GetService<IApiVersionDescriptionProvider>());

await app.RunAsync();
