using System;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class PutPetViewModel : PetBaseViewModel, IEntity<Guid>
{
    [FromBody]
    public Guid Id { get; set; }
}
