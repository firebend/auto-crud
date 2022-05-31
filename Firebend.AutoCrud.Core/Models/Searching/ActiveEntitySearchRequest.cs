using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.Searching
{
    public class ActiveEntityRequest : EntityRequest, IActiveEntitySearchRequest
    {
        /// <inheritdoc />
        public bool? IsDeleted { get; set; }
    }

    public class ActiveEntitySearchRequest : ActiveEntityRequest, IFullTextSearchRequest
    {
        /// <inheritdoc />
        public string Search { get; set; }
    }
}
