using System.Collections.Generic;

namespace Firebend.AutoCrud.Core.Models
{
    public class EntitySearchRequest
    {
        public int? PageNumber { get; set; }

        public int? PageSize { get; set; }
        
        public IEnumerable<EntityOrderDefinition> OrderBy { get; set; }
        
        public string Search { get; set; }
    }
}