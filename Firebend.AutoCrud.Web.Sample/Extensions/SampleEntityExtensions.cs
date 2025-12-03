using System;
using System.Linq;
using Firebend.AutoCrud.ChangeTracking.EntityFramework;
using Firebend.AutoCrud.ChangeTracking.Mongo;
using Firebend.AutoCrud.ChangeTracking.Web;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.CustomFields.EntityFramework;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.CustomFields.Mongo;
using Firebend.AutoCrud.CustomFields.Web;
using Firebend.AutoCrud.DomainEvents.MassTransit.Extensions;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Elastic.Extensions;
using Firebend.AutoCrud.Io;
using Firebend.AutoCrud.Io.Web;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Mongo.Models;
using Firebend.AutoCrud.Web.Sample.Authorization.Handlers;
using Firebend.AutoCrud.Web.Sample.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Elastic;
using Firebend.AutoCrud.Web.Sample.Models;
using Firebend.AutoCrud.Web.Sample.ValidationServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Firebend.AutoCrud.Web.Sample.Extensions;

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
                    .WithMongoChangeTracking(changeTracking => changeTracking.WithConnectionString(configuration.GetConnectionString("Mongo")))
                    .WithMassTransit()
                    .WithDomainEventEntityAddedSubscriber<MongoPersonDomainEventHandler>()
                    .WithDomainEventEntityUpdatedSubscriber<MongoPersonDomainEventHandler>())
                .AddCrud(crud => crud
                    .WithSearchHandler<CustomSearchParametersMongo, MongoCustomSearchHandler>()
                    .WithCrud()
                )
                .AddMongoPersonApiV1()
                .AddMongoPersonApiV2()

        );

    private static EntityCrudBuilder<Guid, MongoTenantPerson> AddMongoPersonApiV1(this EntityCrudBuilder<Guid, MongoTenantPerson> builder) =>
        builder
            .AddIo<Guid, MongoTenantPerson, V1>(io => io.WithMapper(x => new PersonExport(x)))
            .AddControllers<Guid, MongoTenantPerson, V1>(controllers => controllers
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
                .AddChangeTrackingResourceAuthorization()
                .AddCustomFieldsResourceAuthorization()
                .AddResourceAuthorization()
                .WithAllControllers(true)
                .WithIoControllers()
                .WithOpenApiGroupName("The Beautiful Mongo People")
                .WithCustomFieldsControllers(openApiName: "The Beautiful Mongo People Custom Fields")
                .WithChangeTrackingControllers(openApiName: "The Beautiful Mongo People Change Tracking")
                .WithVersionedRoute("mongo-person", "api", deprecated: true)
                .Builder
                .WithRegistration<ICustomFieldsValidationService<Guid, MongoTenantPerson, V1>,
                    CustomFieldValidationService<Guid, MongoTenantPerson, V1>>()
            );

    // ReSharper disable once UnusedMethodReturnValue.Local
    private static EntityCrudBuilder<Guid, MongoTenantPerson> AddMongoPersonApiV2(this EntityCrudBuilder<Guid, MongoTenantPerson> builder) =>
        builder
            .AddIo<Guid, MongoTenantPerson, V2>(io => io.WithMapper(x => new PersonExport(x)))
            .AddControllers<Guid, MongoTenantPerson, V2>(controllers => controllers
                .WithReadViewModel(x => new GetPersonViewModelV2(x))
                .WithCreateViewModel<CreatePersonViewModelV2>(x =>
                {
                    var mongoTenantPerson = new MongoTenantPerson();
                    x.Body.CopyPropertiesTo(mongoTenantPerson);
                    mongoTenantPerson.FirstName = x.Body.Name.First;
                    mongoTenantPerson.LastName = x.Body.Name.Last;
                    mongoTenantPerson.NickName = x.Body.Name.NickName;
                    return mongoTenantPerson;
                })
                .WithUpdateViewModel<CreatePersonViewModelV2, PersonViewModelBaseV2>(vm =>
                    {
                        var mongoTenantPerson = new MongoTenantPerson();
                        vm.Body.CopyPropertiesTo(mongoTenantPerson);
                        mongoTenantPerson.FirstName = vm.Body.Name.First;
                        mongoTenantPerson.LastName = vm.Body.Name.Last;
                        mongoTenantPerson.NickName = vm.Body.Name.NickName;
                        return mongoTenantPerson;
                    },
                    entity =>
                    {
                        var vm = new CreatePersonViewModelV2 { Body = new PersonViewModelBaseV2() };
                        entity.CopyPropertiesTo(vm.Body);
                        vm.Body.Name = new Name
                        {
                            First = entity.FirstName,
                            Last = entity.LastName,
                            NickName = entity.NickName
                        };
                        return vm;
                    })
                .WithCreateMultipleViewModel<CreateMultiplePeopleViewModelV2, PersonViewModelBaseV2>((_, vm) =>
                {
                    var mongoTenantPerson = new MongoTenantPerson();
                    vm.CopyPropertiesTo(mongoTenantPerson);
                    mongoTenantPerson.FirstName = vm.Name.First;
                    mongoTenantPerson.LastName = vm.Name.Last;
                    mongoTenantPerson.NickName = vm.Name.NickName;
                    return mongoTenantPerson;
                })
                .WithAllControllers(true)
                .WithIoControllers()
                .AddChangeTrackingResourceAuthorization()
                .AddCustomFieldsResourceAuthorization()
                .AddResourceAuthorization()
                .WithOpenApiGroupName("The Beautiful Mongo People")
                .WithCustomFieldsControllers(openApiName: "The Beautiful Mongo People Custom Fields")
                .WithChangeTrackingControllers(openApiName: "The Beautiful Mongo People Change Tracking")
                .WithVersionedRoute("mongo-person", "api")
                .Builder
                .WithRegistration<ICustomFieldsValidationService<Guid, MongoTenantPerson, V2>,
                    CustomFieldValidationService<Guid, MongoTenantPerson, V2>>()
            );

    public static EntityFrameworkEntityCrudGenerator AddEfPerson(this EntityFrameworkEntityCrudGenerator generator,
        IConfiguration configuration) =>
        generator.AddEntity<Guid, EfPerson>(person =>
            person.WithIncludes(x => x.Include(y => y.CustomFields)
                    .Include(y => y.Pets)
                    .ThenInclude(y => y.CustomFields)
                    .AsSplitQuery())
                .AddElasticPool(manager =>
                    {
                        manager.ConnectionString = configuration.GetConnectionString("Elastic");
                        manager.MapName = configuration["Elastic:MapName"];
                        manager.Server = configuration["Elastic:ServerName"];
                        manager.ElasticPoolName = configuration["Elastic:PoolName"];
                    }, pool => pool.WithShardKeyProvider<SampleKeyProvider>()
                        .WithAllShardKeyProvider<SampleKeyProvider>()
                        .WithShardDbNameProvider<SampleDbNameProvider>()
                )
                .AddCustomFields(cf =>
                    cf.WithSearchHandler<EntitySearchAuthorizationHandler<Guid,
                            EfPerson, CustomFieldsSearchRequest>>()
                        .AddCustomFieldsTenant<int>(c => c.AddDomainEvents(de => de.WithEfChangeTracking().WithMassTransit()).AddControllers<Guid, EfCustomFieldsModelTenant<Guid, EfPerson, int>, V1>(controllers => controllers
                            .WithChangeTrackingControllers()
                            .WithVersionedRoute("ef-person/{personId:guid}/custom-fields", "api")
                            .WithOpenApiGroupName("The Beautiful Sql People Custom Fields")
                            .WithOpenApiEntityName("Person Custom Field", "Person Custom Fields"))))
                .AddCrud(crud => crud
                    .WithSearchHandler<CustomSearchParameters, EfCustomSearchHandler>()
                    .WithCrud()
                )
                .AddDomainEvents(events => events
                    .WithEfChangeTracking()
                    .WithMassTransit()
                    .WithDomainEventEntityAddedSubscriber<EfPersonDomainEventHandler>()
                    .WithDomainEventEntityUpdatedSubscriber<EfPersonDomainEventHandler>()
                )
                .AddIo<Guid, EfPerson, V1>(io => io.WithMapper(x => new PersonExport(x)))
                .AddControllers<Guid, EfPerson, V1>(controllers => controllers
                    .WithCreateViewModel<CreatePersonViewModel>(view => new EfPerson(view))
                    .WithUpdateViewModel<CreatePersonViewModel, PersonViewModelBase>(
                        view => new EfPerson(view),
                        entity => new CreatePersonViewModel { Body = new PersonViewModelBase(entity) })
                    .WithReadViewModel<GetPersonViewModel, PersonViewModelMapper>()
                    .WithCreateMultipleViewModel<CreateMultiplePeopleViewModel, PersonViewModelBase>(
                        (_, viewModel) => new EfPerson(viewModel))
                    .WithAllControllers(true)
                    .AddResourceAuthorization()
                    .AddChangeTrackingResourceAuthorization()
                    .AddCustomFieldsResourceAuthorization()
                    .WithIoControllers()
                    .WithOpenApiGroupName("The Beautiful Sql People")
                    .WithChangeTrackingControllers(openApiName: "The Beautiful Sql People Change Tracking")
                    .WithCustomFieldsControllers(openApiName: "The Beautiful Sql People Custom Fields")
                    .WithMaxPageSize(20)
                    .WithMaxExportPageSize(50)
                    .WithVersionedRoute(routePrefix: "api")
                    .WithValidationService<PersonValidationService>()
                    .WithDeleteValidationService<PersonDeleteValidationService>()
                    //.AddAuthorizationPolicies()
                    .Builder
                    .WithRegistration<ICustomFieldsValidationService<Guid, EfPerson, V1>,
                        CustomFieldValidationService<Guid, EfPerson, V1>>()
                ));

    public static EntityFrameworkEntityCrudGenerator AddEfPets(this EntityFrameworkEntityCrudGenerator generator,
        IConfiguration configuration) =>
        generator.AddEntity<Guid, EfPet>(person =>
            person.WithIncludes(pets => pets.Include(x => x.Person).AsSplitQuery())
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
                            de.WithEfChangeTracking()
                                .WithMassTransit();
                        }).AddControllers<Guid, EfCustomFieldsModelTenant<Guid, EfPet, int>, V1>(controllers => controllers
                            .WithChangeTrackingControllers()
                            .WithVersionedRoute("ef-person/{personId:guid}/pets/{petId:guid}/custom-fields", "api")
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
                .AddIo<Guid, EfPet, V1>(io => io.WithMapper(x => new ExportPetViewModel(x)))
                .AddControllers<Guid, EfPet, V1>(controllers => controllers
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
                    .WithAllControllers(true)
                    .WithChangeTrackingControllers()
                    .WithCustomFieldsControllers()
                    .WithIoControllers()
                    .WithVersionedRoute("ef-person/{personId:guid}/pets", "api")
                    .WithOpenApiGroupName("The Beautiful Fur Babies")


                ));
}
