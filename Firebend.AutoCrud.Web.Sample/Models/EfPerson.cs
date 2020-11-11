namespace Firebend.AutoCrud.Web.Sample.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Core.Extensions;
    using Core.Interfaces.Models;

    [Table("EfPeople")]
    public class EfPerson : IEntity<Guid>, IActiveEntity, IModifiedEntity, ITenantEntity<int>
    {
        [StringLength(250)]
        public string FirstName { get; set; }

        [StringLength(250)]
        public string LastName { get; set; }

        [Key] public Guid Id { get; set; }

        [StringLength(100)]
        public string NickName { get; set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public int TenantId { get; set; }

        public EfPerson()
        {

        }

        public EfPerson(PersonViewModel viewModel)
        {
            viewModel.CopyPropertiesTo(this);
        }
    }
}
