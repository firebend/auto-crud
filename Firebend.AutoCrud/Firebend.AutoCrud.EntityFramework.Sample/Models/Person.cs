using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.EntityFramework.Sample.Models
{
    [Table("People")]
    public class Person : IEntity<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        
        [StringLength(250)]
        [Required]
        public string FirstName { get; set; }
        
        [StringLength(250)]
        [Required]
        public string LastName { get; set; }
        
        public ICollection<Pet> Pets { get; set; }
    }
}