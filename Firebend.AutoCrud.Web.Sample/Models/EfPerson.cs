using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Sample.Models
{
    [Table("EfPeople")]
    public class EfPerson : IEntity<Guid>, IActiveEntity, IModifiedEntity, ITenantEntity<int>
    {
        public EfPerson()
        {
        }

        public EfPerson(PersonViewModel viewModel)
        {
            viewModel.Body.CopyPropertiesTo(this);
        }

        [StringLength(250)]
        public string FirstName { get; set; }

        [StringLength(250)]
        public string LastName { get; set; }

        [StringLength(100)]
        public string NickName { get; set; }

        public bool IsDeleted { get; set; }

        [Key]
        public Guid Id { get; set; }

        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public int TenantId { get; set; }

        public ICollection<EfPet> Pets { get; set; }
    }
}
