using System.Collections.Generic;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Models
{
    public class MultipleEntityViewModel<T> : IMultipleEntityViewModel<T>
    {
        [FromBody]
        public IEnumerable<T> Entities { get; set; }
    }
}
