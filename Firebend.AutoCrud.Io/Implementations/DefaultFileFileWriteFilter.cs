using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Implementations;

public class DefaultFileFileWriteFilter<TExport, TVersion> : IFileFieldWriteFilter<TExport, TVersion>
    where TVersion : class, IAutoCrudApiVersion
{
    public bool ShouldExport(IFileFieldWrite<TExport> field) => true;
}
