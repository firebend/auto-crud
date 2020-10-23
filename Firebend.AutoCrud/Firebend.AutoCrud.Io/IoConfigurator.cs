using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Interfaces.Models;
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
            Builder.WithRegistration<IEntityExportMapper<TEntity, TEntity>, DefaultEntityExportMapper<TEntity>>();
            
            AddExportEntityRegistrations<TEntity>();
        }

        private void AddExportEntityRegistrations<T>() where T : class
        {
            Builder.WithRegistration<IEntityExportService<T>, EntityExportService<T>>();
            Builder.WithRegistration<IFileFieldAutoMapper<T>, FileFieldAutoMapper<T>>();
            Builder.WithRegistration<IFileFieldWriteFilter<T>, DefaultFileFileWriteFilter<T>>();
        }

        private void RemoveExportEntityRegistrations<T>() where T : class
        {
            Builder.Registrations.Remove(typeof(IEntityExportService<T>));
            Builder.Registrations.Remove(typeof(IFileFieldAutoMapper<T>));
            Builder.Registrations.Remove(typeof(IFileFieldWriteFilter<T>));
            Builder.Registrations.Remove(typeof(IEntityExportMapper<T, T>));
        }

        public IoConfigurator<TBuilder, TKey, TEntity> WithMapper<TOut, TMapper>()
            where TOut : class
            where TMapper : IEntityExportMapper<TEntity, TOut>
        {
            Builder.ExportType = typeof(TOut);
            Builder.WithRegistration<IEntityExportMapper<TEntity, TOut>, TMapper>();
            RemoveExportEntityRegistrations<TEntity>();
            AddExportEntityRegistrations<TOut>();
            return this;
        }
    }
}