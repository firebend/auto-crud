using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Models
{
    public class CreatePersonViewModel
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

    public class PersonViewModelBase : ICustomFieldsEntity<Guid>
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
        public string LastName { get; set; }

        [StringLength(100)]
        public string NickName { get; set; }

        [StringLength(300)]
        public string Email { get; set; }

        [StringLength(300)]
        public string OtherEmail { get; set; }

        public List<CustomFieldsEntity<Guid>> CustomFields { get; set; }
    }

    public class GetPersonViewModel : PersonViewModelBase
    {
        public GetPersonViewModel(EfPerson entity)
        {
            if (entity == null)
            {
                return;
            }

            entity.CopyPropertiesTo(this, nameof(CustomFields));

            CustomFields = entity.CustomFields?.Select(x => new CustomFieldsEntity<Guid>(x)).ToList();
        }

        public Guid Id { get; set; }

        public bool IsDeleted { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }
    }

    public class CreateMultiplePeopleViewModel : IMultipleEntityViewModel<PersonViewModelBase>
    {
        [FromBody]
        public IEnumerable<PersonViewModelBase> Entities { get; set; }
    }
}
