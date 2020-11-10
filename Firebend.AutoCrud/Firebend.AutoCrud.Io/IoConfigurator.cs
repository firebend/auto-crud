using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Io.Abstractions;
using Firebend.AutoCrud.Io.Implementations;
using Firebend.AutoCrud.Io.Interfaces;

namespace Firebend.AutoCrud.Io
{
    public class IoConfigurator<TBuilder, TKey, TEntity> :
        EntityBuilderConfigurator<TBuilder, TKey, TEntity>
        where TBuilder : EntityBuilder<TKey, TEntity>
        where TKey : struct where TEntity : class, IEntity<TKey>
    {
        public IoConfigurator(TBuilder builder) : base(builder)
        {
            builder.WithRegistration<IEntityFileTypeMimeTypeMapper, EntityFileTypeMimeTypeMapper>();
            builder.WithRegistration<IEntityFileWriterCsv, CsvEntityFileWriter>();
            builder.WithRegistration<IEntityFileWriterSpreadSheet, SpreadSheetEntityFileWriter>();
            builder.WithRegistration<IEntityFileWriterFactory, EntityFileWriterFactory>();
            Builder.WithRegistration<IEntityExportMapper<TEntity, TEntity>, AbstractDefaultEntityExportMapper<TEntity>>();
            
            AddExportEntityRegistrations<TEntity>();
        }

        private void AddExportEntityRegistrations<TOut>() where TOut : class
        {
            Builder.ExportType = typeof(TOut);
            Builder.WithRegistration<IEntityExportService<TOut>, EntityExportService<TOut>>();
            Builder.WithRegistration<IFileFieldAutoMapper<TOut>, FileFieldAutoMapper<TOut>>();
            Builder.WithRegistration<IFileFieldWriteFilter<TOut>, AbstractDefaultFileFileWriteFilter<TOut>>();
        }

        private void RemoveExportEntityRegistrations<TOut>() where TOut : class
        {
            Builder.Registrations.Remove(typeof(IEntityExportService<TOut>));
            Builder.Registrations.Remove(typeof(IFileFieldAutoMapper<TOut>));
            Builder.Registrations.Remove(typeof(IFileFieldWriteFilter<TOut>));
            Builder.Registrations.Remove(typeof(IEntityExportMapper<TOut, TOut>));
        }

        public IoConfigurator<TBuilder, TKey, TEntity> WithMapper<TOut, TMapper>()
            where TOut : class
            where TMapper : IEntityExportMapper<TEntity, TOut>
        {
            Builder.WithRegistration<IEntityExportMapper<TEntity, TOut>, TMapper>();
            
            RemoveExportEntityRegistrations<TEntity>();
            AddExportEntityRegistrations<TOut>();
            
            return this;
        }

        public IoConfigurator<TBuilder, TKey, TEntity> WithMapper<TOut>(Func<TEntity, TOut> mapper)
            where TOut : class
        {
            Builder.WithRegistrationInstance(new EntityExportMapper<TEntity, TOut>
            {
                MapperFunc = mapper
            });
            
            RemoveExportEntityRegistrations<TEntity>();
            AddExportEntityRegistrations<TOut>();
            
            return this;
        }
    }
}