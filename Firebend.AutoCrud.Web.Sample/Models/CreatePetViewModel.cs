using System;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class CreatePetViewModel : IEntityViewModelCreate<PetBaseViewModel>
{
    [FromRoute(Name = "personId")]
    public Guid PersonId { get; set; }

    [FromBody]
    public PetBaseViewModel Body { get; set; }
}
