using System;
using Firebend.AutoCrud.ChangeTracking.EntityFramework;
using Firebend.AutoCrud.ChangeTracking.Mongo;
using Firebend.AutoCrud.ChangeTracking.Web;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.DomainEvents.MassTransit.Extensions;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Elastic.Extensions;
using Firebend.AutoCrud.Io;
using Firebend.AutoCrud.Io.Web;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Web.Sample.DbContexts;
using Firebend.AutoCrud.Web.Sample.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Elastic;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.Extensions.Configuration;

namespace Firebend.AutoCrud.Web.Sample.Extensions
{
    public static class SampleEntityExtensions
    {
        public static MongoEntityCrudGenerator AddMongoPerson(this MongoEntityCrudGenerator generator)
        {
            return generator.AddEntity<Guid, MongoPerson>(person =>
                person.WithDefaultDatabase("Samples")
                    .WithCollection("People")
                    .WithFullTextSearch()
                    .AddDomainEvents(domainEvents => domainEvents
                        .WithMongoChangeTracking()
                        .WithMassTransit())
                    .AddCrud(x => x.WithCrud().WithOrderBy(m => m.LastName))
                    .AddControllers(controllers => controllers
                        .WithAllControllers(true)
                        .WithChangeTrackingControllers()
                        .WithOpenApiGroupName("The Beautiful Mongo People"))
                );
        }

        public static EntityFrameworkEntityCrudGenerator AddEfPerson(this EntityFrameworkEntityCrudGenerator generator,
            IConfiguration configuration)
        {
            return generator.AddEntity<Guid, EfPerson>(person =>
                person.WithDbContext<PersonDbContext>()
                    .WithSearchFilter((efPerson, s) => efPerson.LastName.Contains(s) || efPerson.FirstName.Contains(s))
                    .AddElasticPool(manager =>
                        {
                            manager.ConnectionString = configuration.GetConnectionString("Elastic");
                            manager.MapName = configuration["Elastic:MapName"];
                            manager.Server = configuration["Elastic:ServerName"];
                            manager.ElasticPoolName = configuration["Elastic:PoolName"];
                        }, pool => pool
                            .WithShardKeyProvider<SampleKeyProvider>()
                            .WithShardDbNameProvider<SampleDbNameProvider>()
                    )
                    .AddCrud(crud => crud
                        .WithCrud()
                        .WithOrderBy(efPerson => efPerson.LastName))
                    .AddDomainEvents(events => events
                        .WithEfChangeTracking()
                        .WithMassTransit()
                        .WithDomainEventEntityAddedSubscriber<EfPersonDomainEventHandler>()
                        .WithDomainEventEntityUpdatedSubscriber<EfPersonDomainEventHandler>()
                    )
                    .AddIo( io => io.WithMapper(x => new EfPersonExport(x)))
                    .AddControllers(controllers => controllers
                        .WithAllControllers(true)
                        .WithOpenApiGroupName("The Beautiful Sql People")
                        .WithChangeTrackingControllers()
                        .WithIoControllers()
                    ));
        }
    }
}