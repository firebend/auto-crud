using System;
using Firebend.AutoCrud.Core.Models.Searching;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class PetSearch : EntitySearchRequest
{
    [FromRoute] public Guid? PersonId { get; set; }
}
