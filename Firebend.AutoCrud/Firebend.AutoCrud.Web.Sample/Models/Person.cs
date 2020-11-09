using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Sample.Models
{
    public class MongoPerson : IEntity<Guid>, IActiveEntity, IModifiedEntity
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public Guid Id { get; set; }
        
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
    }

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
    }
}