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
