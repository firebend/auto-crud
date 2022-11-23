using System;
using Firebend.AutoCrud.Web.Sample;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

var processId = Environment.ProcessId;
Console.WriteLine($"Auto Crud Web Sample is running on process id {processId}");

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets("Firebend.AutoCrud");

Startup.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();
Startup.Configure(app, app.Environment);

await app.RunAsync();
