using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.Searching
{
    public class ActiveModifiedEntitySearchRequest : EntitySearchRequest, IActiveEntitySearchRequest, IModifiedEntitySearchRequest
    {
        public bool? IsDeleted { get; set; }

        public DateTimeOffset? CreatedStartDate { get; set; }
        public DateTimeOffset? CreatedEndDate { get; set; }
        public DateTimeOffset? ModifiedStartDate { get; set; }
        public DateTimeOffset? ModifiedEndDate { get; set; }
    }
}
