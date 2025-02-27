using System.Collections.Generic;

namespace Firebend.AutoCrud.Web.Interfaces;

public interface IMultipleEntityViewModel<T>
{
    public IEnumerable<T> Entities { get; set; }
}
