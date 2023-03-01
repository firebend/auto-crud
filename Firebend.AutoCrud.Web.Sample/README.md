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

We need to define the connection strings into `secrets` file or `appsettings.json` file)

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

```mermaid
    graph TD;
    id1[Configure Authentication]
    id2[Create Resource Authorization Handlers]
    id3[Register Auth Policies]
    id4[Register Auth Handlers]
    
    id1-->id2;
    id2-->id3;
    id3-->id4;
```

#### Authentication
For the authentication, on the sample project we set the token statically on the `Startup.cs` file for the test purpose as below;

```c#
  app.Use(async (context, next) =>
            {
                if (context.Request.Headers.All(h => h.Key != AuthorizationHeaderKey))
                {
                    context.Request.Headers.Add(AuthorizationHeaderKey, AuthorizationHeaderValue);
                }

                await next(context);
            });
```

You may want configure it for your own authentication logic.

#### Authorization

For the `resource authorization`, we created the Authorization Handlers.

- [ChangeTrackingAuthorizationHandler.cs](./Authorization/Handlers/ChangeTrackingAuthorizationHandler.cs)
- [CreateAuthorizationHandler.cs](./Authorization/Handlers/CreateAuthorizationHandler.cs)
- [CreateMultipleAuthorizationHandler.cs](./Authorization/Handlers/CreateMultipleAuthorizationHandler.cs)
- [CustomFieldsAuthorizationHandler](./Authorization/Handlers/CustomFieldsAuthorizationHandler.cs)
- [DeleteAuthorizationHandler.cs](./Authorization/Handlers/DeleteAuthorizationHandler.cs)
- [ReadAllAuthorizationHandler.cs](./Authorization/Handlers/ReadAllAuthorizationHandler.cs)
- [ReadAuthorizationHandler.cs](./Authorization/Handlers/ReadAuthorizationHandler.cs)
- [EntitySearchAuthorizationHandler.cs](./Authorization/Handlers/EntitySearchAuthorizationHandler.cs)
- [UpdateAuthorizationHandler.cs](./Authorization/Handlers/UpdateAuthorizationHandler.cs)

These handlers are injected on the startup file. 

```c#
// startup.cs

services
    ...
    .AddDefaultResourceAuthorizationRequirements()
    .AddResourceAuthorizationHandlers();

```

You can inject different handlers for the resource policies. If you want to use the existing handlers, you may want to add your own business logic into them.

As a data contract, we created a sample interface as [IDataAuth](./Models/IDataAuth.cs). This interface can carry the properties for addressing the authorization business logic.

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
                        .AddChangeTrackingResourceAuthorization()
                        .AddCustomFieldsResourceAuthorization()
                    )
            );

```

To handle resource authorization for search, you can add custom search handlers.

```c#
 public static EntityFrameworkEntityCrudGenerator AddEfPerson(this EntityFrameworkEntityCrudGenerator generator,
            IConfiguration configuration) =>
            generator.AddEntity<Guid, EfPerson>(person =>
                person.WithDbContext<PersonDbContext>()
                    ...
                    .AddCrud(crud => crud
                        .WithSearchHandler<CustomSearchParameters, EfCustomSearchHandler>()
                        .WithCrud()
                    )
```

To handle resource authorization for custom field search, you can add custom search handlers.

```c#
generator.AddEntity<Guid, EfPerson>(person =>
                person.WithDbContext<PersonDbContext>()
                    ....
                    .AddCustomFields(cf =>
                        cf.WithSearchHandler<EntitySearchAuthorizationHandler<Guid,
                                EfPerson, CustomFieldsSearchRequest>>()
                            .AddCustomFieldsTenant<int>(c => c.AddDomainEvents(de =>
                            {
                                de.WithEfChangeTracking(new ChangeTrackingOptions { PersistCustomContext = true })
                                    .WithMassTransit();
                            }).AddControllers(controllers => controllers
                                .WithChangeTrackingControllers()
                                .WithVersionedRoute("ef-person/{personId}/custom-fields")
                                .WithOpenApiGroupName("The Beautiful Sql People Custom Fields")
                                .WithOpenApiEntityName("Person Custom Field", "Person Custom Fields"))))
```

