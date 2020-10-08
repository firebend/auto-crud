using System.Collections.Generic;

namespace Firebend.AutoCrud.Core.Models.Searching
{
    public class EntitySearchRequest
    {
        public int? PageNumber { get; set; }

        public int? PageSize { get; set; }
        
        public IEnumerable<string> OrderBy { get; set; }
        
        public string Search { get; set; }

        public bool DoCount { get; set; } = true;
    }
}