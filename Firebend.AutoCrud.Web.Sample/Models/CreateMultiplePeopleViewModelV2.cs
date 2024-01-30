using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class CreateMultiplePeopleViewModelV2 : IEntityViewModelCreateMultiple<PersonViewModelBaseV2>
{
    [FromBody]
    public IEnumerable<PersonViewModelBaseV2> Entities { get; set; }
}
