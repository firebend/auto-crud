using System.Collections.Generic;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.IntegrationTests.Models;

public class MultiResult<T>
{
    public IEnumerable<T> Created { get; set; }

    public IEnumerable<ModelStateResult<T>> Errors { get; set; }
}
