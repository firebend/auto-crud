
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.Searching
{
    public class EntityRequest : IOrderableSearchRequest
    {
        /// <inheritdoc />
        public int? PageNumber { get; set; }

        /// <inheritdoc />
        public int? PageSize { get; set; }

        /// <inheritdoc />
        public string[] OrderBy { get; set; }

        /// <inheritdoc />
        public bool? DoCount { get; set; } = true;
    }

    /// <inheritdoc />
    public class EntitySearchRequest : EntityRequest, IFullTextSearchRequest
    {
        /// <inheritdoc />
        public string Search { get; set; }
    }
}
