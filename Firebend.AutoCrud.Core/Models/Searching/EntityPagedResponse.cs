using System.Collections.Generic;

namespace Firebend.AutoCrud.Core.Models.Searching
{
    public class EntityPagedResponse<TEntity>
    {
        /// <summary>
        /// The collection of entities on this given page.
        /// </summary>
        public IEnumerable<TEntity> Data { get; set; }

        /// <summary>
        /// The total number of entities.
        /// </summary>
        public long? TotalRecords { get; set; }

        /// <summary>
        /// The current page.
        /// </summary>
        public int? CurrentPage { get; set; }

        /// <summary>
        /// The current page size.
        /// </summary>
        public int? CurrentPageSize { get; set; }
    }
}
