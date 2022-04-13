using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IFileFieldAutoMapper<T>
        where T : class
    {
        IFileFieldWrite<T>[] MapOutput();
    }

    public interface IDefaultFileFieldAutoMapper
    {
        IFileFieldWrite<T>[] MapOutput<T>()
            where T : class;
    }
}
