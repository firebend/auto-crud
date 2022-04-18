using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IFileFieldAutoMapper
    {
        IFileFieldWrite<T>[] MapOutput<T>()
            where T : class;
    }
}
