using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Abstractions
{
    public abstract class AbstractDefaultFileFileWriteFilter<TExport> : IFileFieldWriteFilter<TExport>
    {
        public bool ShouldExport(IFileFieldWrite<TExport> field)
        {
            return true;
        }
    }
}
