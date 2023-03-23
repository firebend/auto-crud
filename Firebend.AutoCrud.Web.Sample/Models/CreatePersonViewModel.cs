using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Models;

public static class PersonExtensions
{
    public static GetPersonViewModel ToViewModel(this EfPerson person) => new(person);
    public static GetPersonViewModel ToViewModel(this MongoTenantPerson person) => new(person);
}

public class CreatePersonViewModel : IEntityViewModelCreate<PersonViewModelBase>, IViewModelWithBody<PersonViewModelBase>
{
    public CreatePersonViewModel()
    {

    }

    public CreatePersonViewModel(EfPerson entity)
    {
        Body = new PersonViewModelBase(entity);
    }

    public CreatePersonViewModel(MongoPerson entity)
    {
        Body = new PersonViewModelBase(entity);
    }

    [FromBody]
    public PersonViewModelBase Body { get; set; }
}

public class CreatePersonViewModelV2 : IEntityViewModelCreate<PersonViewModelBaseV2>, IViewModelWithBody<PersonViewModelBaseV2>
{
    public CreatePersonViewModelV2()
    {

    }

    public CreatePersonViewModelV2(EfPerson entity)
    {
        Body = new PersonViewModelBaseV2(entity);
    }

    public CreatePersonViewModelV2(MongoPerson entity)
    {
        Body = new PersonViewModelBaseV2(entity);
    }

    [FromBody]
    public PersonViewModelBaseV2 Body { get; set; }
}

public class PersonViewModelBase : IEntityViewModelBase
{
    public PersonViewModelBase()
    {
    }

    public PersonViewModelBase(EfPerson person)
    {
        person.CopyPropertiesTo(this);
    }

    public PersonViewModelBase(MongoPerson person)
    {
        person.CopyPropertiesTo(this);
    }

    [StringLength(250)]
    public string FirstName { get; set; }

    [StringLength(250)]
    [Required]
    public string LastName { get; set; }

    [StringLength(100)]
    public string NickName { get; set; }

    [StringLength(300)]
    public string Email { get; set; }

    [StringLength(300)]
    public string OtherEmail { get; set; }

    public DataAuth DataAuth { get; set; }
}

public class PersonViewModelBaseV2 : IEntityViewModelBase
{
    public PersonViewModelBaseV2()
    {
    }

    public PersonViewModelBaseV2(EfPerson person)
    {
        person.CopyPropertiesTo(this);
        Name = new Name
        {
            First = person.FirstName,
            Last = person.LastName,
            NickName = person.NickName
        };
    }

    public PersonViewModelBaseV2(MongoPerson person)
    {
        person.CopyPropertiesTo(this);
        Name = new Name
        {
            First = person.FirstName,
            Last = person.LastName,
            NickName = person.NickName
        };
    }

    public Name Name { get; set; }

    [StringLength(300)]
    public string Email { get; set; }

    [StringLength(300)]
    public string OtherEmail { get; set; }

    public DataAuth DataAuth { get; set; }
}

public class GetPersonViewModel : PersonViewModelBase, IEntityViewModelRead<Guid>, ICustomFieldsEntity<Guid>
{
    private static readonly string[] Ignores = { nameof(CustomFields) };
    public List<CustomFieldsEntity<Guid>> CustomFields { get; set; }

    public GetPersonViewModel()
    {

    }

    public GetPersonViewModel(EfPerson entity)
    {
        if (entity == null)
        {
            return;
        }

        entity.CopyPropertiesTo(this, Ignores);

        CustomFields = entity.CustomFields?.Select(x => new CustomFieldsEntity<Guid>(x)).ToList();
    }

    public GetPersonViewModel(MongoTenantPerson entity)
    {
        entity?.CopyPropertiesTo(this);
    }

    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset ModifiedDate { get; set; }
}

public class GetPersonViewModelV2 : PersonViewModelBaseV2, IEntityViewModelRead<Guid>, ICustomFieldsEntity<Guid>
{
    private static readonly string[] Ignores = { nameof(CustomFields) };
    public List<CustomFieldsEntity<Guid>> CustomFields { get; set; }

    public GetPersonViewModelV2()
    {

    }

    public GetPersonViewModelV2(EfPerson entity)
    {
        if (entity == null)
        {
            return;
        }

        entity.CopyPropertiesTo(this, Ignores);

        CustomFields = entity.CustomFields?.Select(x => new CustomFieldsEntity<Guid>(x)).ToList();

        Name = new Name
        {
            First = entity.FirstName,
            Last = entity.LastName,
            NickName = entity.NickName
        };
    }

    public GetPersonViewModelV2(MongoTenantPerson entity)
    {
        entity?.CopyPropertiesTo(this);
        Name = new Name
        {
            First = entity?.FirstName,
            Last = entity?.LastName,
            NickName = entity?.NickName
        };
    }

    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset ModifiedDate { get; set; }
}

public class Name
{
    public string First { get; set; }
    public string Last { get; set; }
    public string NickName { get; set; }
}

public class CreateMultiplePeopleViewModel : IEntityViewModelCreateMultiple<PersonViewModelBase>
{
    [FromBody]
    public IEnumerable<PersonViewModelBase> Entities { get; set; }
}

public class CreateMultiplePeopleViewModelV2 : IEntityViewModelCreateMultiple<PersonViewModelBaseV2>
{
    [FromBody]
    public IEnumerable<PersonViewModelBaseV2> Entities { get; set; }
}
