using System.Collections.Generic;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Web.Models
{
    public class CreateMultipleActionResult<TEntity, TViewModel>
    {
        public List<TViewModel> Created { get; set; }

        public List<ModelStateResult<TEntity>> Errors { get; set; }
    }
}
