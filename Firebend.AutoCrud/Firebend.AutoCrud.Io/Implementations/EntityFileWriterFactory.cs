using System;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Io.Implementations
{
    public class EntityFileWriterFactory : IEntityFileWriterFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public EntityFileWriterFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEntityFileWriter Get(EntityFileType type)
        {
            using var scope = _serviceProvider.CreateScope();
            
            return type switch
            {
                EntityFileType.Csv => scope.ServiceProvider.GetService<IEntityFileWriterCsv>(),
                EntityFileType.Spreadsheet => scope.ServiceProvider.GetService<IEntityFileWriterSpreadSheet>(),
                _ => throw new Exception($"Could not find file writer for {type}")
            };
        }
    }
}