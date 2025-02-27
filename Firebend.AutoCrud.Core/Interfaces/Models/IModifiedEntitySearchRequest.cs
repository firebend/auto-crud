using System;

namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface IModifiedEntitySearchRequest
{
    public DateTimeOffset? CreatedStartDate { get; set; }
    public DateTimeOffset? CreatedEndDate { get; set; }
    public DateTimeOffset? ModifiedStartDate { get; set; }
    public DateTimeOffset? ModifiedEndDate { get; set; }
}
