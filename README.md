![Build, Test, and Release](https://github.com/firebend/auto-crud/workflows/Build,%20Test,%20and%20Release/badge.svg)

# Firebend.AutoCrud
A framework that provides Create Read Update Delete (CRUD) dotnet core services.

"Extension Packs" are provided to support:
- Entity Framework
- Mongo DB
- Sharded entity framework sql server contexts
- ASP.NET core controllers
  * HTTP GET, PUT, POST, PATCH RESTFul service endpoints
- Domain Events
  * Service bus operations using [MassTransit](https://masstransit-project.com/)
- Change Tracking ( Entity Change Audits )
  * Ability to persist entity changes to mongo db or an entity framework context
- Exporting entities to csv and spreadsheets
  * Uses [CsvHelper](https://joshclose.github.io/CsvHelper/) and [ClosedXml](https://github.com/ClosedXML/ClosedXML)

# Firebend Nuget Page
[https://nuget.org/profiles/firebend](https://www.nuget.org/profiles/Firebend)

# Getting Started

The following lessons walk you through configuring a basic version of autocrud for your app, then progressively adding more advanced features. We'll build off of the dotnet WeatherForecast sample project from the dotnet documentation.

## Project Setup

1. [Install dotnet core](https://dotnet.microsoft.com/download/dotnet-core)
   ```bash
   brew install dotnet
   ```
2. [Create a new webapi project](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-5.0&tabs=visual-studio-code)
   ```bash
   dotnet new webapi -o AutoCrudSampleApi
   cd AutoCrudSampleApi

   dotnet dev-certs https --trust # i had to run this (and check "Disable HTTPS verification" in Postman) to get `dotnet watch run` to work
   ```

This creates a new dotnet core sample project, based on the `WeatherForcast` sample by Microsoft.

You should have a model in `WeatherForcast.cs` and a controller in `Controllers/WeatherForcastController.cs`

At this point, you should be able to run the project
```bash
dotnet watch run
```

and hit the api at `http://localhost:5000/weatherforecast` to see a list of forecasts
```json
[
    {
        "date": "2020-12-22T14:12:33.703633-06:00",
        "temperatureC": 31,
        "temperatureF": 87,
        "summary": "Freezing"
    },
    {
        "date": "2020-12-23T14:12:33.703653-06:00",
        "temperatureC": 20,
        "temperatureF": 67,
        "summary": "Warm"
    },
    {
        "date": "2020-12-24T14:12:33.703653-06:00",
        "temperatureC": -9,
        "temperatureF": 16,
        "summary": "Balmy"
    },
    {
        "date": "2020-12-25T14:12:33.703654-06:00",
        "temperatureC": 49,
        "temperatureF": 120,
        "summary": "Warm"
    },
    {
        "date": "2020-12-26T14:12:33.703654-06:00",
        "temperatureC": 13,
        "temperatureF": 55,
        "summary": "Mild"
    }
]
```

## Entity Framework Quickstart

Install `Firebend.AutoCrud.Mongo` by adding the following to `AutoCrudSampleApi.csproj`
```xml
<ItemGroup>
  <ProjectReference Include="Firebend.AutoCrud.Core" />
  <ProjectReference Include="Firebend.AutoCrud.EntityFramework" />
  <ProjectReference Include="Firebend.AutoCrud.EntityFramework.Elastic" />
  <ProjectReference Include="Firebend.AutoCrud.Web" />
</ItemGroup>
```

or install packages via the `dotnet` cli
```bash
dotnet add package Firebend.AutoCrud.Core
dotnet add package Firebend.AutoCrud.EntityFramework
dotnet add package Firebend.AutoCrud.EntityFramework.Elastic
dotnet add package Firebend.AutoCrud.Web
```

Create a new folder called `DbContexts` in your project and add a file called `AppDbContext.cs` with the following contents
```csharp
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCrudSampleApi.DbContexts
{
    public class AppDbContext : DbContext, IDbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        // define a DbSet for each of your models
        public DbSet<WeatherForecast> WeatherForecasts { get; set; }
    }
}
```

Open `Program.cs`; you should see some code resembling the following
```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
   Host.CreateDefaultBuilder(args)
       .ConfigureWebHostDefaults(webBuilder =>
       {
           webBuilder.UseStartup<Startup>();
       });
```

Add the following `using` directives
```csharp
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
```

And modify `CreateHostBuilder` to match this
```csharp

public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder => {
        webBuilder.UseStartup<Startup>();
    })
    .ConfigureServices((hostContext, services) =>
    {
       services
            .AddDbContext<AppDbContext>(
                options => { options.UseSqlServer("connString"); },
                ServiceLifetime.Singleton
            )
            .UsingEfCrud(ef =>
            {
                ef.AddEntity<Guid, WeatherForecast>(forecast => 
                    forecast.WithDbContext<AppDbContext>()
                        .AddCrud()
                        .AddControllers(controllers => controllers
                            .WithAllControllers(true) ) // `true` turns on the `/all` endpoint
                            .WithOpenApiGroupName("WeatherForecasts")
                        )
                );
            })
            .AddRouting()
            .AddSwaggerGen()
            .AddControllers()
            .AddNewtonsoftJson()
            .AddFirebendAutoCrudWeb(services);

        // this prevents having to wrap POST bodies with `entity`
        // like `{ "entity": { "key": "value" } }`
        services.Configure<ApiBehaviorOptions>(o => o.SuppressInferBindingSourcesForParameters = true);
    });

```

Finally, in `WeatherForecast.cs`, modify the model to extend `IEntity`
```csharp
using System;
using Firebend.AutoCrud.Core.Interfaces.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoCrudSampleApi
{
    [Table("WeatherForecasts")] // define a table
    public class WeatherForecast : IEntity<Guid> // implement the `IEntity` interface
    {
        [Key] // define a Guid with the `Key` annotation to complete the IEntity implementation
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        [Required]
        public int TemperatureC { get; set; }

        [StringLength(250)]
        public string Summary { get; set; }
    }
}
```

Now, we need to create and apply some migrations. From the command line, run
```bash
dotnet ef migrations add CreateTable_WeatherForecasts
dotnet ef database update
```

Now you should be able to run the application and hit the endpoints defined below.
```bash
dotnet watch run
```

Here, we've configured Entity Framework by adding AutoCrud to the `WeatherForecast` model that Microsoft provided us. This will
* Create a new Entity Framework table for `WeatherForecasts` model
* Create the following endpoints to interact with our model
  * `GET` `/api/v1/weather-forecast`
  * `POST` `/api/v1/weather-forecast`
  * `GET` `/api/v1/weather-forecast/{id}`
  * `PUT` `/api/v1/weather-forecast/{id}`
  * `PATCH` `/api/v1/weather-forecast/{id}`
  * `DELETE` `/api/v1/weather-forecast/{id}`
  * `GET` `/api/v1/weather-forecast/all`

To enable searching objects in `GET` requests, include the following in `CreateHostBuilder` in the `WithDbContext` callback; return a function returning a boolean value for whether to include the item in the search results
```csharp
// ... previous setup
ef.AddEntity<Guid, WeatherForecast>(forecast => 
    forecast.WithDbContext<AppDbContext>()
        .WithSearchFilter((f, s) => f.Summary.Contains(s))
// ... rest of your setup
```
Now, when appending the query param `Search={your search}` to your url, you'll only get results with `'{your search}'` in the Summary.


## Mongo Quickstart

Install `Firebend.AutoCrud.Mongo` by adding the following to `AutoCrudSampleApi.csproj`
```xml
<ItemGroup>
  <ProjectReference Include="Firebend.AutoCrud.Core" />
  <ProjectReference Include="Firebend.AutoCrud.Mongo" />
  <ProjectReference Include="Firebend.AutoCrud.Web" />
</ItemGroup>
```

or install packages via the `dotnet` cli
```bash
dotnet add package Firebend.AutoCrud.Core
dotnet add package Firebend.AutoCrud.Mongo
dotnet add package Firebend.AutoCrud.Web
```

Open `Program.cs`; you should see some code resembling the following
```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
   Host.CreateDefaultBuilder(args)
       .ConfigureWebHostDefaults(webBuilder =>
       {
           webBuilder.UseStartup<Startup>();
       });
```

Add the following `using` directives
```csharp
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Web;
using Firebend.AutoCrud.Mongo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
```

And modify `CreateHostBuilder` to match this
```csharp
public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
   .ConfigureWebHostDefaults(webBuilder =>
   {
       webBuilder.UseStartup<Startup>();
   })
   .ConfigureServices((hostContext, services) =>
   {
       services
           .UsingMongoCrud("mongodb://localhost:27017", mongo => {
               mongo.AddEntity<Guid, WeatherForecast>(forecast =>
                   forecast.WithDefaultDatabase("Samples")
                       .WithCollection("WeatherForecasts")
                       .AddCrud()
                       .AddControllers(controllers => {
                           controllers
                               .WithAllControllers(true) // `true` turns on the `/all` endpoint
                               .WithOpenApiGroupName("WeatherForecasts");
                       })
               );
           })
           .AddRouting()
           .AddSwaggerGen()
           .AddControllers()
           .AddNewtonsoftJson()
           .AddFirebendAutoCrudWeb(services);

        // this prevents having to wrap POST bodies with `entity`
        // like `{ "entity": { "key": "value" } }`
        services.Configure<ApiBehaviorOptions>(o => o.SuppressInferBindingSourcesForParameters = true);
   });
```

Finally, in `WeatherForecast.cs`, modify the model to extend `IEntity`
```csharp
using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace AutoCrudSampleApi
{
    public class WeatherForecast : IEntity<Guid> // use the `IEntity` interface
    {
        public Guid Id { get; set; } // complete the interface implementation
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public string Summary { get; set; }
    }
}
```

You can delete `Controllers/WeatherForecastController.cs`.

Here, we've configured Mongo by adding AutoCrud to the `WeatherForecast` model that Microsoft provided us. This will
* Create a new Mongo collection called `Samples` with a new collection called `WeatherForecasts`
* Create the following endpoints to interact with our model
  * `GET` `/api/v1/weather-forecast`
  * `POST` `/api/v1/weather-forecast`
  * `GET` `/api/v1/weather-forecast/{id}`
  * `PUT` `/api/v1/weather-forecast/{id}`
  * `PATCH` `/api/v1/weather-forecast/{id}`
  * `DELETE` `/api/v1/weather-forecast/{id}`
  * `GET` `/api/v1/weather-forecast/all`

To enable searching via `GET` endpoints, add `.WithFullTextSearch()` after `WithCollection` in the `CreateHostBuilder` `AddEntity` callback
```csharp
// ... previous setup
mongo.AddEntity<Guid, WeatherForecast>(forecast =>
   forecast.WithDefaultDatabase("Samples")
       .WithCollection("WeatherForecasts")
       .WithFullTextSearch()
       .AddCrud()
       .AddControllers(controllers => {
           controllers
               .WithAllControllers(true) // `true` turns on the `/all` endpoint
               .WithOpenApiGroupName("WeatherForecasts");
       })
// ... rest of your setup
```

Now, when appending the query param `Search={your search}` to your url, you'll only get results with `'{your search}'` in any text field.

## Add Custom Ordering and Search

To enable ordering of results, include the following in `CreateHostBuilder` in the `AddEntity` callback
```csharp
// ... previous setup
forecast.WithDbContext<AppDbContext>()
    .WithSearchFilter((f, s) => f.Summary.Contains(s))
    .AddCrud(crud => crud
        .WithCrud()
        .WithOrderBy(f => f.TemperatureC)
// ... rest of your setup
```

This will sort results by the `TemperatureC` member, in ascending order. To sort in descending order, pass `false` as the second parameter to `WithOrderby`: `.WithOrderBy(f => f.TemperatureC, false)`

To include custom search parameters for an entity, include the following in `CreateHostBuilder` in the `AddEntity` callback; return a function returning a boolean value for whether to include the item in the search results, or `null` to skip the search.
```csharp
// ... previous setup
ef.AddEntity<Guid, WeatherForecast>(forecast => 
    forecast.WithDbContext<AppDbContext>()
        .WithSearchFilter((f, s) => f.Summary.Contains(s))
        .AddCrud(crud => crud
            .WithCrud()
            .WithSearch<CustomSearchParameters>(search => {
                if (!string.IsNullOrWhiteSpace(search?.CustomField))
                {
                    return f => f.Summary.Contains(search.CustomField);
                }

                return null;
            })
        )
// ... rest of your setup
```

Create a new class `CustomSearchParameters` that extends `EntitySearchRequest` and has your custom fields as members
```csharp
using Firebend.AutoCrud.Core.Models.Searching;

namespace AutoCrudSampleApi
{
    public class CustomSearchParameters : EntitySearchRequest
    {
        public string CustomField { get; set; }
    }
}
```

Now, you can append the `CustomField={your search}` query parameter to your url and filter results by a custom parameter.
## Add Entity Export

To create an endpoint that exports your results to xlxs or csv

Install the `Firebend.AutoCrud.Io` and `Firebend.AutoCrud.Io.Web` packages
```xml
<ItemGroup>
<!-- ... rest of the PackageReferences -->
    <PackageReference Include="Firebend.AutoCrud.Io" />
    <PackageReference Include="Firebend.AutoCrud.Io.Web" />
</ItemGroup>
```

or 

```bash
dotnet add package Firebend.AutoCrud.Io
dotnet add package Firebend.AutoCrud.Io.Web
```

Then, add the `AddIo()` before `AddControllers` in the `CreateHostBuilder` `AddEntity` callback and `WithIoControllers()` to the `AddControllers`
```csharp
// ... previous setup
ef.AddEntity<Guid, WeatherForecast>(forecast => 
    forecast.WithDbContext<AppDbContext>()
        .AddCrud()
        .AddIo(io => io.WithMapper(x => new WeatherForecastExport(x)))
        .AddControllers(controllers => controllers
            .WithAllControllers(true)
            .WithOpenApiGroupName("WeatherForecasts")
            .WithIoControllers()
        )
  );
// ... rest of your setup
```

Create a new class `WeatherForecastExport` like the following
```csharp
using System;
using Firebend.AutoCrud.Core.Extensions;

namespace AutoCrudSampleApi
{
    public class WeatherForecastExport
    {
        public WeatherForecastExport()
        {
        }

        public WeatherForecastExport(WeatherForecast forecast)
        {
            forecast.CopyPropertiesTo(this);
        }

        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; }
    }
}
```

Now, you can make a `GET` request to the `/weather-forecast/export/{csv|excel}?filename=test.csv` endpoint, with either `csv` or `excel` requested. A file name is required. All search fields and custom parameters that work on the `GET` `/` and `GET` `/all` endpoints work here too.

## Add Change Tracking

Install the `Firebend.AutoCrud.ChangeTracking` and `Firebend.AutoCrud.DomainEvents` packages
```xml
<!-- other packages -->
<PackageReference Include="MassTransit.AspNetCore" />
<PackageReference Include="MassTransit.RabbitMQ" />
<PackageReference Include="Firebend.AutoCrud.ChangeTracking" />
<PackageReference Include="Firebend.AutoCrud.ChangeTracking.EntityFramework" />
<PackageReference Include="Firebend.AutoCrud.ChangeTracking.Mongo" />
<PackageReference Include="Firebend.AutoCrud.ChangeTracking.Web" />
<PackageReference Include="Firebend.AutoCrud.DomainEvents.MassTransit" />
```

or 

```bash
dotnet add package MassTransit.AspNetCore
dotnet add package MassTransit.RabbitMQ
dotnet add package Firebend.AutoCrud.ChangeTracking
dotnet add package Firebend.AutoCrud.ChangeTracking.EntityFramework # or Firebend.AutoCrud.ChangeTracking.Mongo
dotnet add package Firebend.AutoCrud.ChangeTracking.Web
dotnet add package Firebend.AutoCrud.DomainEvents.MassTransit
```

Modify the `ConfigureServices` callback in `CreateHostBuilder` like so
```csharp
services
    .AddDbContext<AppDbContext>(options => { options.UseSqlServer(
        "Data Source=localhost;Initial Catalog=master;Persist Security Info=True;User ID=sa;Password=Password0#@!;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;MultipleActiveResultSets=True;Max Pool Size=200;"); }, // make sure `Persist Security Info=True` in your connection string
        ServiceLifetime.Singleton
    )
    .UsingEfCrud(ef =>
    {
        ef.AddEntity<Guid, WeatherForecast>(forecast => 
            forecast.WithDbContext<AppDbContext>()
                .AddCrud()
                .AddDomainEvents(events => events // add this
                    .WithEfChangeTracking()
                    .WithMassTransit()
                )
                .AddControllers(controllers => controllers
                    .WithAllControllers(true)
                    .WithOpenApiGroupName("WeatherForecasts")
                    .WithChangeTrackingControllers() // and this
                )
        )
        .WithDomainEventContextProvider<DomainEventContextProvider>(); // and this
    })
    .AddMassTransit(hostContext.Configuration) // and this
    .AddRouting()
    .AddSwaggerGen()
    .AddControllers()
    .AddNewtonsoftJson()
    .AddFirebendAutoCrudWeb(services);
```

Now, create a `DomainEventContextProvider` class like below
```csharp
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace AutoCrudSampleApi
{
    public class CatchPhraseModel
    {
        public string CatchPhrase { get; set; }
    }

    public class DomainEventContextProvider : IDomainEventContextProvider
    {
        public DomainEventContext GetContext() => new DomainEventContext
        {
            Source = "My Sample",
            UserEmail = "sample@firebend.com",
            CustomContext = new CatchPhraseModel { CatchPhrase = "I Like Turtles" }
        };
    }
}
```

Finally, create a class `MassTransitExtensions` with the following
```csharp
using System;
using System.Text.RegularExpressions;
using Firebend.AutoCrud.DomainEvents.MassTransit.Extensions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCrudSampleApi
{
    public static class MassTransitExtensions
    {
        private static readonly Regex ConStringParser = new Regex(
            "^rabbitmq://([^:]+):(.+)@([^@]+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static IServiceCollection AddMassTransit(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var connString = "rabbitmq://guest:guest@localhost/"; // replace with your connection string

            if (string.IsNullOrWhiteSpace(connString))
            {
                throw new Exception("Please configure a service bus connection string for Rabbit MQ");
            }

            return serviceCollection.AddMassTransit(bus =>
                {
                    bus.RegisterFirebendAutoCrudDomainEventHandlers(serviceCollection);

                    bus.UsingRabbitMq((context, configurator) =>
                    {
                        var match = ConStringParser.Match(connString);

                        var domain = match.Groups[3].Value;
                        var uri = $"rabbitmq://{domain}";

                        configurator.Host(new Uri(uri), h =>
                        {
                            h.PublisherConfirmation = true;
                            h.Username(match.Groups[1].Value);
                            h.Password(match.Groups[2].Value);
                        });

                        configurator.Lazy = true;
                        configurator.AutoDelete = true;
                        configurator.PurgeOnStartup = true;

                        context.RegisterFirebendAutoCrudeDomainEventHandlerEndPoints(configurator, serviceCollection);
                    });
                })
                .AddMassTransitHostedService();
        }
    }
}
```

Now, you should be able to hit the endpoint `/weather-forecast/{id}/changes` and get a list of modifications to the object. You can add new entries by `PUT`ing to `/weather-forecast/{id}` to change some values.
## Elastic pool

```
// todo
```

## Sharding

```
// todo
```


A complete working example can be found [here](Firebend.AutoCrud/Firebend.AutoCrud.Web.Sample) that utilizes all the extension packs mentioned above. Sample `.http` files are also provided with example HTTP requests. [Get the VS Code REST client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)