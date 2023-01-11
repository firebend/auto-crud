using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.EntityFramework;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Mongo;
using Firebend.AutoCrud.ChangeTracking.Web;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.CustomFields.EntityFramework;
using Firebend.AutoCrud.CustomFields.Mongo;
using Firebend.AutoCrud.CustomFields.Web;
using Firebend.AutoCrud.DomainEvents.MassTransit.Extensions;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Elastic.Extensions;
using Firebend.AutoCrud.Io;
using Firebend.AutoCrud.Io.Web;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Mongo.Models;
using Firebend.AutoCrud.Web.Interfaces;
using Firebend.AutoCrud.Web.Sample.Authorization.Handlers;
using Firebend.AutoCrud.Web.Sample.DbContexts;
using Firebend.AutoCrud.Web.Sample.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Elastic;
using Firebend.AutoCrud.Web.Sample.Models;
using Firebend.AutoCrud.Web.Sample.ValidationServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Firebend.AutoCrud.Web.Sample.Extensions
{
    public static class SampleEntityExtensions
    {
        public static MongoEntityCrudGenerator AddMongoPerson(this MongoEntityCrudGenerator generator, IConfiguration configuration) =>
            generator
                .AddEntity<Guid, MongoTenantPerson>(person =>
                person
                    .WithConnectionString(configuration.GetConnectionString("Mongo"))
                    .WithDefaultDatabase("Samples")
                    .WithCollection("People")
                    .WithFullTextSearch()
                    .WithShardKeyProvider<SampleKeyProviderMongo>()
                    .WithAllShardsProvider<SampleAllShardsMongoProvider>()
                    .WithShardMode(MongoTenantShardMode.Database)
                    .AddCustomFields(cf => cf
                        .WithSearchHandler<EntitySearchAuthorizationHandler<Guid,
                            MongoTenantPerson, CustomFieldsSearchRequest>>()
                        .WithCustomFields()
                    )
                    .AddDomainEvents(domainEvents => domainEvents
                        .WithMongoChangeTracking(changeTracking => changeTracking.WithConnectionString(configuration.GetConnectionString("Mongo")),
                            new ChangeTrackingOptions { PersistCustomContext = true })
                        .WithMassTransit()
                        .WithDomainEventEntityAddedSubscriber<MongoPersonDomainEventHandler>()
                        .WithDomainEventEntityUpdatedSubscriber<MongoPersonDomainEventHandler>())
                    .AddCrud(crud => crud
                        .WithSearchHandler<CustomSearchParameters, MongoCustomSearchHandler>()
                        .WithCrud()
                    )
                    .AddIo(io => io.WithMapper(x => new PersonExport(x)))
                    .AddControllers(controllers => controllers
                        .WithReadViewModel(x => new GetPersonViewModel(x))
                        .WithCreateViewModel<CreatePersonViewModel>(x =>
                        {
                            var mongoTenantPerson = new MongoTenantPerson();
                            x.Body.CopyPropertiesTo(mongoTenantPerson);
                            return mongoTenantPerson;
                        })
                        .WithUpdateViewModel<CreatePersonViewModel, PersonViewModelBase>(vm =>
                        {
                            var mongoTenantPerson = new MongoTenantPerson();
                            vm.Body.CopyPropertiesTo(mongoTenantPerson);
                            return mongoTenantPerson;
                        },
                        entity =>
                        {
                            var vm = new CreatePersonViewModel { Body = new PersonViewModelBase() };
                            entity.CopyPropertiesTo(vm.Body);
                            return vm;
                        })
                        .WithCreateMultipleViewModel<CreateMultiplePeopleViewModel, PersonViewModelBase>((_, vm) =>
                        {
                            var mongoTenantPerson = new MongoTenantPerson();
                            vm.CopyPropertiesTo(mongoTenantPerson);
                            return mongoTenantPerson;
                        })
                        .WithAllControllers(true)
                        .WithIoControllers()
                        .WithCustomFieldsControllers(openApiName: "The Beautiful Mongo People Custom Fields")
                        .WithChangeTrackingControllers()
                        .AddChangeTrackingResourceAuthorization()
                        .AddCustomFieldsResourceAuthorization()
                        .AddResourceAuthorization()
                        .WithOpenApiGroupName("The Beautiful Mongo People")
                        .WithRoute("api/v1/mongo-person")
                        .Builder
                        .WithRegistration<ICustomFieldsValidationService<Guid, MongoTenantPerson>,
                            CustomFieldValidationService<Guid, MongoTenantPerson>>()
                    )
            );

        public static EntityFrameworkEntityCrudGenerator AddEfPerson(this EntityFrameworkEntityCrudGenerator generator,
            IConfiguration configuration) =>
            generator.AddEntity<Guid, EfPerson>(person =>
                person.WithDbContext<PersonDbContext>()
                    .WithDbOptionsProvider<PersonDbContextOptionsProvider<Guid, EfPerson>>()
                    .WithIncludes(x => x.Include(y => y.CustomFields)
                        .Include(y => y.Pets)
                        .ThenInclude(y => y.CustomFields))
                    .AddElasticPool(manager =>
                        {
                            manager.ConnectionString = configuration.GetConnectionString("Elastic");
                            manager.MapName = configuration["Elastic:MapName"];
                            manager.Server = configuration["Elastic:ServerName"];
                            manager.ElasticPoolName = configuration["Elastic:PoolName"];
                        }, pool => pool.WithShardKeyProvider<SampleKeyProvider>()
                            .WithShardDbNameProvider<SampleDbNameProvider>()
                    )
                    .AddCustomFields(cf =>
                        cf.WithSearchHandler<EntitySearchAuthorizationHandler<Guid,
                                EfPerson, CustomFieldsSearchRequest>>()
                            .AddCustomFieldsTenant<int>(c => c.AddDomainEvents(de =>
                            {
                                de.WithEfChangeTracking(new ChangeTrackingOptions { PersistCustomContext = true })
                                    .WithMassTransit();
                            }).AddControllers(controllers => controllers
                                .WithChangeTrackingControllers()
                                .WithRoute("/api/v1/ef-person/{personId:guid}/custom-fields")
                                .WithOpenApiGroupName("The Beautiful Sql People Custom Fields")
                                .WithOpenApiEntityName("Person Custom Field", "Person Custom Fields"))))
                    .AddCrud(crud => crud
                        .WithSearchHandler<CustomSearchParameters, EfCustomSearchHandler>()
                        .WithCrud()
                    )
                    .AddDomainEvents(events => events
                        .WithEfChangeTracking(new ChangeTrackingOptions { PersistCustomContext = true })
                        .WithMassTransit()
                        .WithDomainEventEntityAddedSubscriber<EfPersonDomainEventHandler>()
                        .WithDomainEventEntityUpdatedSubscriber<EfPersonDomainEventHandler>()
                    )
                    .AddIo(io => io.WithMapper(x => new PersonExport(x)))
                    .AddControllers(controllers => controllers
                        .WithCreateViewModel<CreatePersonViewModel>(view => new EfPerson(view))
                        .WithUpdateViewModel<CreatePersonViewModel, PersonViewModelBase>(
                            view => new EfPerson(view),
                            entity => new CreatePersonViewModel { Body = new PersonViewModelBase(entity) })
                        .WithReadViewModel<GetPersonViewModel, PersonViewModelMapper>()
                        //.WithReadViewModel(entity => new GetPersonViewModel(entity))
                        .WithCreateMultipleViewModel<CreateMultiplePeopleViewModel, PersonViewModelBase>(
                            (_, viewModel) => new EfPerson(viewModel))
                        .WithAllControllers(true)
                        .AddResourceAuthorization()
                        .WithOpenApiGroupName("The Beautiful Sql People")
                        .WithChangeTrackingControllers()
                        .AddChangeTrackingResourceAuthorization()
                        .AddCustomFieldsResourceAuthorization()
                        .WithCustomFieldsControllers(openApiName: "The Beautiful Sql People Custom Fields")
                        .WithIoControllers()
                        .WithMaxPageSize(20)
                        .WithMaxExportPageSize(50)
                        .WithValidationService<PersonValidationService>()
                        .Builder
                        .WithRegistration<ICustomFieldsValidationService<Guid, EfPerson>,
                            CustomFieldValidationService<Guid, EfPerson>>()
                    ));

        public static EntityFrameworkEntityCrudGenerator AddEfPets(this EntityFrameworkEntityCrudGenerator generator,
            IConfiguration configuration) =>
            generator.AddEntity<Guid, EfPet>(person =>
                person.WithDbContext<PersonDbContext>()
                    .WithDbOptionsProvider<PersonDbContextOptionsProvider<Guid, EfPet>>()
                    .WithIncludes(pets => pets.Include(x => x.Person))
                    .AddElasticPool(manager =>
                        {
                            manager.ConnectionString = configuration.GetConnectionString("Elastic");
                            manager.MapName = configuration["Elastic:MapName"];
                            manager.Server = configuration["Elastic:ServerName"];
                            manager.ElasticPoolName = configuration["Elastic:PoolName"];
                        }, pool => pool.WithShardKeyProvider<SampleKeyProvider>()
                            .WithShardDbNameProvider<SampleDbNameProvider>()
                    )
                    .AddCustomFields(cf =>
                        cf.WithSearchHandler<EntitySearchAuthorizationHandler<Guid,
                                EfPet, CustomFieldsSearchRequest>>()
                            .AddCustomFieldsTenant<int>(c => c.AddDomainEvents(de =>
                            {
                                de.WithEfChangeTracking(new ChangeTrackingOptions { PersistCustomContext = true })
                                    .WithMassTransit();
                            }).AddControllers(controllers => controllers
                                .WithChangeTrackingControllers()
                                .WithRoute("/api/v1/ef-person/{personId:guid}/pets/{petId:guid}/custom-fields")
                                .WithOpenApiGroupName("The Beautiful Sql Fur Babies Custom Fields")
                                .WithOpenApiEntityName("Fur Babies Custom Field", "Fur Babies Custom Fields"))))
                    .AddCrud(crud => crud.WithSearchHandler<PetSearch>((pets, parameters) =>
                    {
                        if (!string.IsNullOrWhiteSpace(parameters.Search))
                        {
                            pets = pets.Where(x => x.PetName.Contains(parameters.Search));
                        }

                        pets = pets.Where(x => x.EfPersonId == parameters.PersonId);

                        return pets;
                    }).WithCrud())
                    .AddDomainEvents(events => events
                        .WithEfChangeTracking()
                        .WithMassTransit()
                    )
                    .AddIo(io => io.WithMapper(x => new ExportPetViewModel(x)))
                    .AddControllers(controllers => controllers
                        .WithReadViewModel(pet => new GetPetViewModel(pet))
                        .WithCreateViewModel<CreatePetViewModel>(pet => new EfPet(pet))
                        .WithUpdateViewModel<PutPetViewModel, PetBaseViewModel>(
                            pet => new EfPet(pet),
                            entity =>
                            {
                                var pet = new PutPetViewModel();
                                entity.CopyPropertiesTo(pet);
                                return pet;
                            })
                        .WithRoute("/api/v1/ef-person/{personId:guid}/pets")
                        .WithAllControllers(true)
                        .WithOpenApiGroupName("The Beautiful Fur Babies")
                        .WithChangeTrackingControllers()
                        .WithIoControllers()
                        .WithCustomFieldsControllers()
                    ));
    }

    public class PersonViewModelMapper : IReadViewModelMapper<Guid, EfPerson, GetPersonViewModel>
    {
        public Task<EfPerson> FromAsync(GetPersonViewModel model, CancellationToken cancellationToken = default) =>
            null;

        public Task<IEnumerable<EfPerson>> FromAsync(IEnumerable<GetPersonViewModel> model,
            CancellationToken cancellationToken = default) => null;

        public Task<GetPersonViewModel> ToAsync(EfPerson entity, CancellationToken cancellationToken = default) =>
            Task.FromResult(new GetPersonViewModel(entity));

        public Task<IEnumerable<GetPersonViewModel>> ToAsync(IEnumerable<EfPerson> entity,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(entity.Select(x => new GetPersonViewModel(x)));
    }
}
