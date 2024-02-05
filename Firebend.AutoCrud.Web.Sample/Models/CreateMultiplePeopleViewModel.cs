using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class CreateMultiplePeopleViewModel : IEntityViewModelCreateMultiple<PersonViewModelBase>
{
    [FromBody]
    public IEnumerable<PersonViewModelBase> Entities { get; set; }
}
