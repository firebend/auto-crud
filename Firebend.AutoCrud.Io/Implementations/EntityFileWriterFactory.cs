using System;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Io.Implementations
{
    public class EntityFileWriterFactory : BaseDisposable, IEntityFileWriterFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private IEntityFileWriter _writer;

        public EntityFileWriterFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEntityFileWriter Get(EntityFileType type)
        {
            _writer = GetImpl(type);
            return _writer;
        }

        private IEntityFileWriter GetImpl(EntityFileType type) => type switch
        {
            EntityFileType.Csv => _serviceProvider.GetService<IEntityFileWriterCsv>(),
            EntityFileType.Spreadsheet => _serviceProvider.GetService<IEntityFileWriterSpreadSheet>(),
            EntityFileType.Unknown => throw new Exception($"{nameof(EntityFileType.Unknown)} is not a valid export type."),
            _ => throw new Exception($"Could not find file writer for {type}")
        };

        protected override void DisposeManagedObjects() => _writer?.Dispose();
    }
}
