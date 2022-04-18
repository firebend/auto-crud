using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Firebend.AutoCrud.Core.Attributes;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.Web.Sample.Models
{
    public class MongoPerson : IEntity<Guid>, IActiveEntity, IModifiedEntity, IEntityDataAuth
    {
        public MongoPerson()
        {
        }

        public MongoPerson(CreatePersonViewModel viewModel)
        {
            viewModel?.Body.CopyPropertiesTo(this);
        }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [AutoCrudIgnoreUpdate]
        public string IgnoreMe { get; set; }

        public bool IsDeleted { get; set; }
        public Guid Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public string NickName { get; set; }
        public DataAuth DataAuth { get; set; }
    }

    public class MongoTenantPerson : MongoPerson, ITenantEntity<int>, ICustomFieldsEntity<Guid>
    {
        public int TenantId { get; set; }
        public List<CustomFieldsEntity<Guid>> CustomFields { get; set; }
    }
}
