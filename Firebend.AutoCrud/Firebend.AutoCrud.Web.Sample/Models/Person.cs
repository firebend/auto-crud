using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Sample.Models
{
    public class MongoPerson : IEntity<Guid>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public Guid Id { get; set; }
    }

    [Table("EfPeople")]
    public class EfPerson : IEntity<Guid>
    {
        [StringLength(250)] public string FirstName { get; set; }

        [StringLength(250)] public string LastName { get; set; }

        [Key] public Guid Id { get; set; }
    }
}