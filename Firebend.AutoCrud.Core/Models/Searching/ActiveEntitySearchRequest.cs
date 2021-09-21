using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.Searching
{
    public class ActiveEntitySearchRequest : EntitySearchRequest, IActiveEntitySearchRequest
    {
        /// <summary>
        /// True if including for deleted entities. False for including active entities. Null for including all entities.
        /// </summary>
        public bool? IsDeleted { get; set; }
    }
}
