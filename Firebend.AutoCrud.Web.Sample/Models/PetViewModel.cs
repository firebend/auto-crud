using System;
using System.ComponentModel.DataAnnotations;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Models
{
    public class PetBaseViewModel
    {
        [Required]
        [MaxLength(205)]
        public string PetName { get; set; }

        [Required]
        [MaxLength(250)]
        public string PetType { get; set; }
    }

    public class CreatePetViewModel
    {
        [FromRoute(Name = "personId")]
        public Guid PersonId { get; set; }

        [FromBody]
        public PetBaseViewModel Body { get; set; }
    }

    public class PutPetViewModel : PetBaseViewModel, IEntity<Guid>
    {
        [FromBody]
        public Guid Id { get; set; }
    }

    public class PetPersonViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid Id { get; set; }
    }

    public class GetPetViewModel : PutPetViewModel
    {
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public PetPersonViewModel Person { get; set; }

        public GetPetViewModel()
        {

        }

        public GetPetViewModel(EfPet pet)
        {
            pet.CopyPropertiesTo(this);

            if (pet.Person != null)
            {
                this.Person = new PetPersonViewModel();
                pet.Person.CopyPropertiesTo(this.Person);
            }
        }
    }

    public class ExportPetViewModel : PutPetViewModel
    {
        public string PersonFirstName { get; set; }

        public string PersonLastName { get; set; }

        public ExportPetViewModel()
        {

        }

        public ExportPetViewModel(EfPet pet)
        {
            pet.CopyPropertiesTo(this);
            PersonFirstName = pet.Person?.FirstName;
            PersonLastName = pet.Person?.LastName;
        }
    }

}
