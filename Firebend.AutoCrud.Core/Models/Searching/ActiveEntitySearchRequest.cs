using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.Searching
{
    public class ActiveEntitySearchRequest : EntitySearchRequest, IActiveEntitySearchRequest
    {
        public bool? IsDeleted { get; set; }
    }
}
