using System.Collections.Generic;

namespace Firebend.AutoCrud.Core.Models.Searching
{
    public class EntityPagedResponse<TEntity>
    {
        public IEnumerable<TEntity> Data { get; set; }

        public long? TotalRecords { get; set; }

        public int? CurrentPage { get; set; }

        public int? CurrentPageSize { get; set; }
    }
}
