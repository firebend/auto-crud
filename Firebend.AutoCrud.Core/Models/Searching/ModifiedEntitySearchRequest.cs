using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.Searching;

public class ModifiedEntitySearchRequest : ModifiedEntityRequest, IFullTextSearchRequest
{
    /// <inheritdoc />
    public string Search { get; set; }
}
