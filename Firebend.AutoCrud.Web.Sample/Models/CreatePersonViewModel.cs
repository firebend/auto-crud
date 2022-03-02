using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Models
{
    public class CreatePersonViewModel: IEntityViewModelCreate<PersonViewModelBase>
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

    public class PersonViewModelBase: IEntityViewModelBase
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

        public DataAuth DataAuth { get; set; }
    }

    public class GetPersonViewModel : PersonViewModelBase, IEntityViewModelRead<EfPerson>, ICustomFieldsEntity<Guid>
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

            // this.CustomFields = entity.CustomFields;
            // this.FirstName = entity.FirstName;
            // this.NickName = entity.NickName;
            // this.Id = entity.Id;
            // this.CreatedDate = entity.CreatedDate;
            // this.ModifiedDate = entity.ModifiedDate;
            // this.IsDeleted = entity.IsDeleted;
            // this.OtherEmail = entity.OtherEmail;

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

    public class CreateMultiplePeopleViewModel : IEntityViewModelCreateMultiple<PersonViewModelBase>
    {
        [FromBody]
        public IEnumerable<PersonViewModelBase> Entities { get; set; }
    }
}
