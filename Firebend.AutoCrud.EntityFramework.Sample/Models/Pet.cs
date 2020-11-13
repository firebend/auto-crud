using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.EntityFramework.Sample.Models
{
    [Table("Pets")]
    public class Pet : IEntity<Guid>
    {
        [StringLength(250)]
        [Required]
        public string Name { get; set; }

        [Required]
        public Guid PersonId { get; set; }

        [Key]
        public Guid Id { get; set; }
    }
}
