using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces;

public interface IFileFieldAutoMapper<TVersion>
    where TVersion : class, IAutoCrudApiVersion
{
    public IFileFieldWrite<T>[] MapOutput<T>()
        where T : class;
}
