using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IFileFieldWriteFilter<TExport>
    {
        bool ShouldExport(IFileFieldWrite<TExport> field);
    }
}
