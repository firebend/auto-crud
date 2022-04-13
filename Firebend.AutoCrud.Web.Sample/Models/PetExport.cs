using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.Web.Sample.Models
{
    public class PetExport : IEntityViewModelExport
    {
        public PetExport()
        {
            Pets1 = new List<EfPet>
            {
                new() { Id = Guid.NewGuid(), PetName = "ted1" },
                new() { Id = Guid.NewGuid(), PetName = "derp1" }
            };
        }

        public PetExport(EfPet pet)
        {
            pet.CopyPropertiesTo(this);
            EfPersonId = pet.EfPersonId;
            Pets1 = new List<EfPet>
            {
                new() { Id = Guid.NewGuid(), PetName = "ted1" },
                new() { Id = Guid.NewGuid(), PetName = "derp1" }
            };
        }
        public ICollection<EfPet> Pets1 { get; set; }

        public Guid Id { get; set; }

        public Guid EfPersonId { get; set; }

        [Required]
        [MaxLength(205)]
        public string PetName { get; set; }

        [Required]
        [MaxLength(250)]
        public string PetType { get; set; }

        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
