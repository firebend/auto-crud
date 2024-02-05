
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.Searching;

/// <inheritdoc cref="EntityRequest" />
public class EntitySearchRequest : EntityRequest, IFullTextSearchRequest
{
    /// <inheritdoc />
    public string Search { get; set; }
}
