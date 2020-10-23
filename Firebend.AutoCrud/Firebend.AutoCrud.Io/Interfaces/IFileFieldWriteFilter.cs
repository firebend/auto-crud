using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IFileFieldWriteFilter<TExport>
    {
        bool ShouldExport(IFileFieldWrite<TExport> field);
    }

    public abstract class DefaultFileFileWriteFilter<TExport> : IFileFieldWriteFilter<TExport>
    {
        public bool ShouldExport(IFileFieldWrite<TExport> field)
        {
            return true;
        }
    }
}