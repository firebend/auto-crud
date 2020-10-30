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

Define an entity using the `IEntity<TKey>` interface

```cs
public class Person : IEntity<Guid>
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LasName { get; set;}
}
```

Choose a data persistence model. Entity Framework or Mongo DB are currently supported. We will demonstrate Mongo.
To add AutoCrud, use fluent extensions to the `IServiceCollection` interface.

```cs
return Host.CreateDefaultBuilder(args)
  .ConfigureServices((hostContext, services) =>
  {
      services
          .UsingMongoCrud(hostContext.Configuration.GetConnectionString("Mongo"),
          mongo =>
          {
            //todo
          })
  }
```

Once inside the configuration callback for the Mongo Crud Generator you can add an entity builder and configure it like so:

```cs
return Host.CreateDefaultBuilder(args)
  .ConfigureServices((hostContext, services) =>
  {
      services.UsingMongoCrud(hostContext.Configuration.GetConnectionString("Mongo"),
          mongo =>
          {
              mongo.AddEntity<Guid, Person>(person =>
              {
                  person.WithDefaultDatabase("MyMongoDb")
                      .WithCollection("MyPersonCollection")
                      .WithFullTextSearch()
                      .AddCrud(crud =>
                      {
                          crud
                            .WithCrud()
                            .WithOrderBy(m => m.LastName));
                      })
                      .AddControllers(controllers => 
                      {
                          controllers
                            .WithAllControllers(true)
                            .WithOpenApiGroupName("People");
                      });
              });
          });
  }
```

In this example we have:
1. Configured a entity to be persisted to Mongo
2. The entity's key is a Guid
3. It will be persisted to the `MyMongoDb` database inside the `MyPersonCollection` collection
4. The entity will have a full text search index for searching. The index will be configured at run time once the collection is created. 
5. We are adding all the CRUD services to Create Read Update and Delete the Person entity. 
6. When providing a list of entities they will be ordered by last name
7. We are adding HTTP GET, POST, PUT, DELETE RESTful endpoints to interact with the entities
8. The controllers with be annotated with [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) annotations. The controllers will be tagged with `People` and grouped together in the Swagger/Open API UI

A complete working example can be found [here](Firebend.AutoCrud/Firebend.AutoCrud.Web.Sample) that utilizes all the extension packs mentioned above. Sample `.http` files are also provided with example HTTP requests. [Get the VS Code REST client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)
