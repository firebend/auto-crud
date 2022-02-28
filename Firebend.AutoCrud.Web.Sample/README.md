[Main README File](https://github.com/firebend/auto-crud/blob/main/README.md)

# Firebend.AutoCrud.Web.Sample

## Getting Started

### Pre-requisites

`libgdiplus.dll`

```shell
brew install mono-libgdiplus
sudo ln -s /opt/homebrew/opt/mono-libgdiplus/lib/libgdiplus.dylib /usr/local/share/dotnet/shared/Microsoft.NETCore.App/6.0.0
```

### Project Setup

We need to define the connection strings (into `secrets` file or `appsettings.json` file)

```json
{
  "ConnectionStrings": {
    "Mongo": "mongodb://localhost:27017",
    "SqlServer": "Data Source=.;Initial Catalog=Firebend_CrudSample;Persist Security Info=False;User ID=sa;Password={your_password};Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;MultipleActiveResultSets=True;Max Pool Size=200;",
    "Elastic": "Data Source=.;Initial Catalog=master;Persist Security Info=False;User ID=sa;Password={your_password};Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;MultipleActiveResultSets=True;Max Pool Size=200;",
    "ServiceBus": "rabbitmq://guest:guest@localhost/"
  },
  "Elastic": {
    "ServerName": ".",
    "MapName": "firebend-autocrud-sample",
    "PoolName": "Firebend Auto Crud Sample"
  }
}
```

We need to run the project.
```shell
dotnet watch run
```

We can run the HTTP request files to populate some data and ensure that controllers are working.

- [sample.ef.http](./sample.ef.http)
- [sample.ef.pets](./sample.ef.pets.http)
- [sample.mongo.http](./sample.mongo.http)

## Authentication / Authorization

### Getting Started
The sample project uses token base `authentication` method. For the `authorization` there are two options as;
- Default
- Resource Base

### Configuration
For the authentication, on the sample project we set the token statically on the `Startup.cs` file for the test purpose as below;

```c#
app.Use(async (context, next) =>
            {
                context.Request.Headers.Add("Authorization",
                    "Bearer eyJhbGciOiJIUzI1NiJ9.eyJSb2xlIjoiQWRtaW4iLCJJc3N1ZXIiOiJJc3N1ZXIiLCJVc2VybmFtZSI6IkphdmFJblVzZSIsImV4cCI6MTY0NTgwMDQwMSwiaWF0IjoxNjQ1ODAwNDAxfQ.XGjDqgMLK-D_X5EZmpeFqslflX6QxEfhCibPLwALP2I");
                await next(context);
            });
```

You may want configure it for your own authentication logic.

For the `resource authorization`, we created the Authorization Handlers.

- [CreateAuthorizationHandler.cs](./Authorization/Handlers/CreateAuthorizationHandler.cs)
- [CreateMultipleAuthorizationHandler.cs](./Authorization/Handlers/CreateMultipleAuthorizationHandler.cs)
- [DeleteAuthorizationHandler.cs](./Authorization/Handlers/DeleteAuthorizationHandler.cs)
- [ReadAllAuthorizationHandler.cs](./Authorization/Handlers/ReadAllAuthorizationHandler.cs)
- [ReadAuthorizationHandler.cs](./Authorization/Handlers/ReadAuthorizationHandler.cs)
- [SearchAuthorizationHandler.cs](./Authorization/Handlers/SearchAuthorizationHandler.cs)
- [UpdateAuthorizationHandler.cs](./Authorization/Handlers/UpdateAuthorizationHandler.cs)

These handlers are injected at the startup file. 

```c#
// startup.cs

services.AddAuthorization(options =>
    {
        options.AddPolicy(ReadAllAuthorizationRequirement.DefaultPolicy,
            policy => policy.Requirements.Add(new ReadAllAuthorizationRequirement()));
        options.AddPolicy(ReadAuthorizationRequirement.DefaultPolicy,
            policy => policy.Requirements.Add(new ReadAuthorizationRequirement()));
        options.AddPolicy(CreateAuthorizationRequirement.DefaultPolicy,
            policy => policy.Requirements.Add(new CreateAuthorizationRequirement()));
        options.AddPolicy(CreateMultipleAuthorizationRequirement.DefaultPolicy,
            policy => policy.Requirements.Add(new CreateMultipleAuthorizationRequirement()));
        options.AddPolicy(UpdateAuthorizationRequirement.DefaultPolicy,
            policy => policy.Requirements.Add(new UpdateAuthorizationRequirement()));
        options.AddPolicy(DeleteAuthorizationRequirement.DefaultPolicy,
            policy => policy.Requirements.Add(new DeleteAuthorizationRequirement()));
    });
            
services.AddSingleton<IAuthorizationHandler, ReadAllAuthorizationHandler>();
services.AddSingleton<IAuthorizationHandler, ReadAuthorizationHandler>();
services.AddSingleton<IAuthorizationHandler, CreateAuthorizationHandler>();
services.AddSingleton<IAuthorizationHandler, CreateMultipleAuthorizationHandler>();
services.AddSingleton<IAuthorizationHandler, UpdateAuthorizationHandler>();
services.AddSingleton<IAuthorizationHandler, DeleteAuthorizationHandler>();
```

You can inject different handlers for the resource policies. If you want to use the existing handlers, you may want to add your own business logic into them.

As a data contract, we created a sample interface as `IDataAuth`. This interface can carry the properties for addressing the authorization business logic.

### Usage

[SampleEntityExtensions.cs](./Extensions/SampleEntityExtensions.cs)

```c#
public static MongoEntityCrudGenerator AddMongoPerson(this MongoEntityCrudGenerator generator) =>
            generator.AddEntity<Guid, MongoTenantPerson>(person =>
                person.WithDefaultDatabase("Samples")
                    ...
                    .AddControllers(controllers => controllers
                        ...
                        .AddResourceAuthorization()
                    )
            );

```
