using System;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Io.Implementations;

public class FileFieldWriteFilterFactory<TVersion> : IFileFieldWriteFilterFactory<TVersion>
    where TVersion : class, IApiVersion
{
    private readonly IServiceProvider _serviceProvider;

    public FileFieldWriteFilterFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IFileFieldWriteFilter<TExport, TVersion> GetFilter<TExport>()
        => _serviceProvider.GetService<IFileFieldWriteFilter<TExport, TVersion>>();
}
