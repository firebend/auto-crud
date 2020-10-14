using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Sample.Models
{
    public class MongoPerson : IEntity<Guid>
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    [Table("EfPeople")]
    public class EfPerson : IEntity<Guid>
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(250)]
        public string FirstName { get; set; }

        [StringLength(250)]
        public string LastName { get; set; }
    }
}