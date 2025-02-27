using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces;

public interface IFileFieldWriteFilter<TExport, TVersion>
    where TVersion : class, IAutoCrudApiVersion
{
    public bool ShouldExport(IFileFieldWrite<TExport> field);
}
