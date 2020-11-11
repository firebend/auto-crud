using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.Searching
{
    public class ModifiedEntitySearchRequest : EntitySearchRequest, IModifiedEntitySearchRequest
    {
        public DateTimeOffset? CreatedStartDate { get; set; }
        public DateTimeOffset? CreatedEndDate { get; set; }

        public DateTimeOffset? ModifiedStartDate { get; set; }
        public DateTimeOffset? ModifiedEndDate { get; set; }
    }
}
