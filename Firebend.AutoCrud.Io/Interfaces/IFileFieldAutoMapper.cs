using System.Collections.Generic;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IFileFieldAutoMapper<T>
        where T : class
    {
        IEnumerable<IFileFieldWrite<T>> MapOutput();
    }
}
