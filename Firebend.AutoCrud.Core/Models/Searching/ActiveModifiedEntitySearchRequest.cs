using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.Searching;

public class ActiveModifiedEntitySearchRequest : ActiveModifiedEntityRequest, IFullTextSearchRequest
{
    /// <inheritdoc />
    public string Search { get; set; }
}
