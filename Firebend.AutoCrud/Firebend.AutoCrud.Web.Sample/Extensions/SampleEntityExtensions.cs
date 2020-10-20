using System;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Elastic.Extensions;
using Firebend.AutoCrud.Generator.Implementations;
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
        public static EntityCrudGenerator<MongoDbEntityBuilder> AddMongoPerson(this EntityCrudGenerator<MongoDbEntityBuilder> generator)
        {
            return generator.AddBuilder<MongoPerson, Guid>(person =>
                person.WithDefaultDatabase("Samples")
                    .WithCollection("People")
                    .WithFullTextSearch()
                    .AddCrud()
                    .AddControllers(controllers => controllers
                        .WithAllControllers(true)
                        .WithOpenApiGroupName("The Beautiful Mongo People"))
                );
        }

        public static EntityCrudGenerator<EntityFrameworkEntityBuilder> AddEfPerson(this EntityCrudGenerator<EntityFrameworkEntityBuilder> generator,
            IConfiguration configuration)
        {
            return generator.AddBuilder<EfPerson, Guid>(person =>
                person.WithDbContext<PersonDbContext>()
                    .WithSearchFilter<EfPerson>((search, p) => p.LastName.Contains(search))
                    //.WithSearchFilter<EfPersonFilter>()  .WithSearchFilter<EfPerson>((search, p) => p.LastName.Contains(search))
                    .AddCrud(crud => crud
                        .WithCrud()
                        .WithOrderBy<EfPersonOrder>())
                    .AddDomainEvents(events => events
                        .WithDomainEventPublisherServiceProvider()
                        .WithDomainEventEntityAddedSubscriber<EfPersonDomainEventSubscriber>()
                        .WithDomainEventEntityUpdatedSubscriber<EfPersonDomainEventSubscriber>()
                    )
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
                    .AddControllers(controllers => controllers
                        .WithAllControllers(true)
                        .WithOpenApiGroupName("The Beautiful Sql People")
                    ));
        }
    }
}