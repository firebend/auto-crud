using System;
using System.Linq;
using Firebend.AutoCrud.ChangeTracking.EntityFramework;
using Firebend.AutoCrud.ChangeTracking.Mongo;
using Firebend.AutoCrud.ChangeTracking.Web;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.DomainEvents.MassTransit.Extensions;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.EntityFramework.CustomCommands;
using Firebend.AutoCrud.EntityFramework.Elastic.Extensions;
using Firebend.AutoCrud.Io;
using Firebend.AutoCrud.Io.Web;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Mongo.Models;
using Firebend.AutoCrud.Web.Sample.DbContexts;
using Firebend.AutoCrud.Web.Sample.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Elastic;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Firebend.AutoCrud.Web.Sample.Extensions
{
    public static class SampleEntityExtensions
    {
        public static MongoEntityCrudGenerator AddMongoPerson(this MongoEntityCrudGenerator generator) =>
            generator.AddEntity<Guid, MongoTenantPerson>(person =>
                person.WithDefaultDatabase("Samples")
                    .WithCollection("People")
                    .WithFullTextSearch()
                    .WithShardKeyProvider<SampleKeyProviderMongo>()
                    .WithAllShardsProvider<SampleAllShardsMongoProvider>()
                    .WithShardMode(MongoTenantShardMode.Database)
                    .AddDomainEvents(domainEvents => domainEvents
                        .WithMongoChangeTracking()
                        .WithMassTransit())
                    .AddCrud()
                    .AddIo(io => io.WithMapper(x => new PersonExport(x)))
                    .AddControllers(controllers => controllers
                        //.WithViewModel(entity => new PersonViewModel(entity), viewModel => new MongoPerson(viewModel))
                        .WithAllControllers(true)
                        .WithChangeTrackingControllers()
                        .WithIoControllers()
                        .WithOpenApiGroupName("The Beautiful Mongo People")
                        .WithRoute("api/v1/mongo-person"))
        );

        public static EntityFrameworkEntityCrudGenerator AddEfPerson(this EntityFrameworkEntityCrudGenerator generator,
            IConfiguration configuration) =>
            generator.AddEntity<Guid, EfPerson>(person =>
                person.WithDbContext<PersonDbContext>()
                    .WithDbOptionsProvider<PersonDbContextOptionsProvider<Guid, EfPerson>>()
                    .AddElasticPool(manager =>
                        {
                            manager.ConnectionString = configuration.GetConnectionString("Elastic");
                            manager.MapName = configuration["Elastic:MapName"];
                            manager.Server = configuration["Elastic:ServerName"];
                            manager.ElasticPoolName = configuration["Elastic:PoolName"];
                        }, pool => pool.WithShardKeyProvider<SampleKeyProvider>()
                            .WithShardDbNameProvider<SampleDbNameProvider>()
                    )
                    .AddCrud(crud => crud
                        .WithSearch<CustomSearchParameters>()
                        .WithCrud()
                        .WithQueryCustomizer<CustomSearchParameters>((query, parameters) =>
                        {
                            if(!string.IsNullOrWhiteSpace(parameters?.NickName))
                            {
                                query = query.Where(x => x.NickName == parameters.NickName);
                            }

                            if (!string.IsNullOrWhiteSpace(parameters?.Search))
                            {
                                query = query.Where(x => EF.Functions.ContainsAny(x.FirstName, parameters.Search));
                            }

                            if (parameters.OrderBy == null)
                            {
                                query = query.OrderBy(x => x.ModifiedDate);
                            }

                            return query;
                        }))
                    .AddDomainEvents(events => events
                        .WithEfChangeTracking()
                        .WithMassTransit()
                        .WithDomainEventEntityAddedSubscriber<EfPersonDomainEventHandler>()
                        .WithDomainEventEntityUpdatedSubscriber<EfPersonDomainEventHandler>()
                    )
                    .AddIo(io => io.WithMapper(x => new PersonExport(x)))
                    .AddControllers(controllers => controllers
                        .WithCreateViewModel<CreatePersonViewModel>(view => new EfPerson(view))
                        .WithUpdateViewModel<CreatePersonViewModel>(view => new EfPerson(view))
                        .WithReadViewModel(entity => new GetPersonViewModel(entity))
                        .WithCreateMultipleViewModel<CreateMultiplePeopleViewModel, PersonViewModelBase>((_, viewModel) => new EfPerson(viewModel))
                        .WithAllControllers(true)
                        .WithOpenApiGroupName("The Beautiful Sql People")
                        .WithChangeTrackingControllers()
                        .WithIoControllers()
                        .WithMaxPageSize(20)
                        .WithMaxExportPageSize(50)
                    ));

        public static EntityFrameworkEntityCrudGenerator AddEfPets(this EntityFrameworkEntityCrudGenerator generator,
            IConfiguration configuration) =>
            generator.AddEntity<Guid, EfPet>(person =>
                person.WithDbContext<PersonDbContext>()
                    .WithDbOptionsProvider<PersonDbContextOptionsProvider<Guid, EfPet>>()
                    .AddElasticPool(manager =>
                        {
                            manager.ConnectionString = configuration.GetConnectionString("Elastic");
                            manager.MapName = configuration["Elastic:MapName"];
                            manager.Server = configuration["Elastic:ServerName"];
                            manager.ElasticPoolName = configuration["Elastic:PoolName"];
                        }, pool => pool.WithShardKeyProvider<SampleKeyProvider>()
                            .WithShardDbNameProvider<SampleDbNameProvider>()
                    )
                    .AddCrud()
                    .AddDomainEvents(events => events
                        .WithEfChangeTracking()
                        .WithMassTransit()
                    )
                    .AddIo(io => io.WithMapper(x => new ExportPetViewModel(x)))
                    .AddControllers(controllers => controllers
                        .WithReadViewModel(pet => new GetPetViewModel(pet))
                        .WithCreateViewModel<CreatePetViewModel>(pet => new EfPet(pet))
                        .WithUpdateViewModel<PutPetViewModel>(pet => new EfPet(pet))
                        .WithRoute("/api/v1/ef-person/{personId}/pets")
                        .WithAllControllers(true)
                        .WithOpenApiGroupName("The Beautiful Fur Babies")
                        .WithChangeTrackingControllers()
                        .WithIoControllers()
                    ));
    }
}
