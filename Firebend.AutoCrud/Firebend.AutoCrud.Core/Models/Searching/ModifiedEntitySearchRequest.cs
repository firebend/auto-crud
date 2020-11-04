using System;

namespace Firebend.AutoCrud.Core.Models.Searching
{
    public class ModifiedEntitySearchRequest: EntitySearchRequest
    {
        public DateTimeOffset? CreatedStartDate { get; set; }
        public DateTimeOffset? CreatedEndDate { get; set; }

        public DateTimeOffset? ModifiedStartDate { get; set; }
        public DateTimeOffset? ModifiedEndDate { get; set; }
    }
}