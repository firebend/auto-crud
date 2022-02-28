using System;
using System.ComponentModel.DataAnnotations;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Io.Attributes;
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

    public class CreatePetViewModel: IDataAuth
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

            if (pet.Person == null)
            {
                return;
            }

            this.Person = new PetPersonViewModel();
            pet.Person.CopyPropertiesTo(this.Person);
        }
    }

    public class ExportPetViewModel
    {
        [Export(Name = "Person Id", Order = 0)]
        public Guid? PersonId { get; set; }

        [Export(Name = "Person First Name", Order = 1)]
        public string PersonFirstName { get; set; }

        [Export(Name = "Person Last  Name", Order = 2)]
        public string PersonLastName { get; set; }

        [Export(Name = "Pet Id", Order = 3)]
        public Guid PetId { get; set; }

        [Export(Name = "Pet Name", Order = 4)]
        public string PetName { get; set; }

        [Export(Name = "Pet Type", Order = 5)]
        public string PetType { get; set; }

        [Export(Name = "Pet Created Date", Order = 6)]
        public DateTimeOffset CreatedDate { get; set; }

        [Export(Name = "Pet Modified Date", Order = 7)]
        public DateTimeOffset ModifiedDate { get; set; }

        [Export(Name = "Pet Deleted", Order = 8)]
        public bool IsDeleted { get; set; }

        public ExportPetViewModel()
        {

        }

        public ExportPetViewModel(EfPet pet)
        {
            pet.CopyPropertiesTo(this);
            PersonFirstName = pet.Person?.FirstName;
            PersonLastName = pet.Person?.LastName;
            PersonId = pet.Person?.Id;
            PetId = pet.Id;
        }
    }

}
