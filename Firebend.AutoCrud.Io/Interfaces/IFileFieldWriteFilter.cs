using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces;

public interface IFileFieldWriteFilter<TExport, TVersion>
    where TVersion : class, IAutoCrudApiVersion
{
    bool ShouldExport(IFileFieldWrite<TExport> field);
}

public interface IFileFieldWriteFilterFactory<TVersion>
    where TVersion : class, IAutoCrudApiVersion
{
    IFileFieldWriteFilter<TExport, TVersion> GetFilter<TExport>();
}
