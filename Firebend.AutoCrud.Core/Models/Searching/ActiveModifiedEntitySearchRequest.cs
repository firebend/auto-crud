using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.Searching
{
    public class ActiveModifiedEntitySearchRequest : EntitySearchRequest, IActiveEntitySearchRequest, IModifiedEntitySearchRequest
    {
        /// <summary>
        /// True if including for deleted entities. False for including active entities. Null for including all entities.
        /// </summary>
        public bool? IsDeleted { get; set; }

        /// <summary>
        /// The earliest time an entity was created.
        /// </summary>
        public DateTimeOffset? CreatedStartDate { get; set; }

        /// <summary>
        /// The latest time an entity was created.
        /// </summary>
        public DateTimeOffset? CreatedEndDate { get; set; }

        /// <summary>
        /// The earliest time an entity was modified.
        /// </summary>
        public DateTimeOffset? ModifiedStartDate { get; set; }

        /// <summary>
        /// The latest time an entity was modified.
        /// </summary>
        public DateTimeOffset? ModifiedEndDate { get; set; }
    }
}
