using System;
using System.ComponentModel.DataAnnotations;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Sample.Models
{
    public class MongoPerson : IEntity<Guid>, IActiveEntity, IModifiedEntity
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

        public bool IsDeleted { get; set; }
        public Guid Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
    }

    public class MongoTenantPerson : MongoPerson, ITenantEntity<int>
    {
        public int TenantId { get; set; }
    }
}
