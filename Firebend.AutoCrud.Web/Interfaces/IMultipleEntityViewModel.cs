using System.Collections.Generic;

namespace Firebend.AutoCrud.Web.Interfaces;

public interface IMultipleEntityViewModel<T>
{
    IEnumerable<T> Entities { get; set; }
}
