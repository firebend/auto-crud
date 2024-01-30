using Firebend.AutoCrud.Core.Interfaces;

namespace Firebend.AutoCrud.Io.Interfaces;

public interface IFileFieldWriteFilterFactory<TVersion>
    where TVersion : class, IAutoCrudApiVersion
{
    IFileFieldWriteFilter<TExport, TVersion> GetFilter<TExport>();
}
