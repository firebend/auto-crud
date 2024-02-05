using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.Searching;

public class ModifiedEntityRequest : EntityRequest, IModifiedEntitySearchRequest
{
    /// <inheritdoc />
    public DateTimeOffset? CreatedStartDate { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? CreatedEndDate { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? ModifiedStartDate { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? ModifiedEndDate { get; set; }
}
