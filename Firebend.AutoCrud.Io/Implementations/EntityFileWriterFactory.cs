using System;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Io.Implementations
{
    public class EntityFileWriterFactory<TVersion> : IEntityFileWriterFactory<TVersion>
        where TVersion : class, IAutoCrudApiVersion
    {
        private readonly IServiceProvider _serviceProvider;

        public EntityFileWriterFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEntityFileWriter<TVersion> Get(EntityFileType type) => type switch
        {
            EntityFileType.Csv => _serviceProvider.GetService<IEntityFileWriterCsv<TVersion>>(),
            EntityFileType.Spreadsheet => _serviceProvider.GetService<IEntityFileWriterSpreadSheet<TVersion>>(),
            EntityFileType.Unknown => throw new Exception($"{nameof(EntityFileType.Unknown)} is not a valid export type."),
            _ => throw new Exception($"Could not find file writer for {type}")
        };
    }
}
