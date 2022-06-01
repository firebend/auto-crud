using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.Searching
{
    public class ActiveModifiedEntityRequest : EntityRequest, IActiveEntitySearchRequest, IModifiedEntitySearchRequest
    {
        /// <inheritdoc />
        public bool? IsDeleted { get; set; }

        /// <inheritdoc />
        public DateTimeOffset? CreatedStartDate { get; set; }

        /// <inheritdoc />
        public DateTimeOffset? CreatedEndDate { get; set; }

        /// <inheritdoc />
        public DateTimeOffset? ModifiedStartDate { get; set; }

        /// <inheritdoc />
        public DateTimeOffset? ModifiedEndDate { get; set; }
    }

    public class ActiveModifiedEntitySearchRequest : ActiveModifiedEntityRequest, IFullTextSearchRequest
    {
        /// <inheritdoc />
        public string Search { get; set; }
    }
}
