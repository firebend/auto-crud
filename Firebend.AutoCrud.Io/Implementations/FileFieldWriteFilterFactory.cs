using System;
using Firebend.AutoCrud.Io.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Io.Implementations;

public class FileFieldWriteFilterFactory : IFileFieldWriteFilterFactory
{
    private readonly IServiceProvider _serviceProvider;

    public FileFieldWriteFilterFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IFileFieldWriteFilter<TExport> GetFilter<TExport>()
        => _serviceProvider.GetService<IFileFieldWriteFilter<TExport>>();
}
