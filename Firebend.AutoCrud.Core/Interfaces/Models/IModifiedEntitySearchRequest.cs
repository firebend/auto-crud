using System;

namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface IModifiedEntitySearchRequest
{
    DateTimeOffset? CreatedStartDate { get; set; }
    DateTimeOffset? CreatedEndDate { get; set; }
    DateTimeOffset? ModifiedStartDate { get; set; }
    DateTimeOffset? ModifiedEndDate { get; set; }
}
