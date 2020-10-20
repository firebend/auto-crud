using System;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Elastic.Extensions;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Web.Sample.DbContexts;
using Firebend.AutoCrud.Web.Sample.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Elastic;
using Firebend.AutoCrud.Web.Sample.Filtering;
using Firebend.AutoCrud.Web.Sample.Models;
using Firebend.AutoCrud.Web.Sample.Ordering;
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
                    .AddCrud()
                    .AddControllers(controllers => controllers
                        .WithAllControllers(true)
                        .WithOpenApiGroupName("The Beautiful Mongo People"))
                );
        }

        public static EntityFrameworkEntityCrudGenerator AddEfPerson(this EntityFrameworkEntityCrudGenerator generator,
            IConfiguration configuration)
        {
            return generator.AddEntity<Guid, EfPerson>(person =>
                person.WithDbContext<PersonDbContext>()
                    .WithSearchFilter<EfPersonFilter>()
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
                        .WithOrderBy<EfPersonOrder>())
                    .AddDomainEvents(events => events
                        .WithDomainEventPublisherServiceProvider()
                        .WithDomainEventEntityAddedSubscriber<EfPersonDomainEventSubscriber>()
                        .WithDomainEventEntityUpdatedSubscriber<EfPersonDomainEventSubscriber>()
                    )
                    .AddControllers(controllers => controllers
                        .WithAllControllers(true)
                        .WithOpenApiGroupName("The Beautiful Sql People")
                    ));
        }
    }
}