using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Io.Implementations;
using Firebend.AutoCrud.Io.Interfaces;

namespace Firebend.AutoCrud.Io;

public class IoConfigurator<TBuilder, TKey, TEntity, TVersion> :
    EntityBuilderConfigurator<TBuilder, TKey, TEntity>
    where TBuilder : EntityBuilder<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
    where TVersion : class, IAutoCrudApiVersion
{
    public IoConfigurator(TBuilder builder) : base(builder)
    {
        builder.WithRegistration<IEntityFileTypeMimeTypeMapper<TVersion>, EntityFileTypeMimeTypeMapper<TVersion>>();
        builder.WithRegistration<IEntityFileWriterCsv<TVersion>, CsvEntityFileWriter<TVersion>>();
        builder.WithRegistration<IEntityFileWriterSpreadSheet<TVersion>, SpreadSheetEntityFileWriter<TVersion>>();
        builder.WithRegistration<IEntityFileWriterFactory<TVersion>, EntityFileWriterFactory<TVersion>>();
        builder.WithRegistration<IEntityExportMapper<TEntity, TVersion, TEntity>, DefaultEntityExportMapper<TEntity, TVersion>>();

        AddExportEntityRegistrations<TEntity>();
    }

    private void AddExportEntityRegistrations<TOut>()
        where TOut : class
    {
        Builder.ExportType = typeof(TOut);
        Builder.WithRegistration<IEntityExportService<TOut, TVersion>, EntityExportService<TOut, TVersion>>();
        Builder.WithRegistration<IFileFieldAutoMapper<TVersion>, FileFieldAutoMapper<TVersion>>();
        Builder.WithRegistration<IFileFieldWriteFilter<TOut, TVersion>, DefaultFileFileWriteFilter<TOut, TVersion>>();
        Builder.WithRegistration<IFileFieldWriteFilterFactory<TVersion>, FileFieldWriteFilterFactory<TVersion>>();
    }

    private void RemoveExportEntityRegistrations<TOut>()
        where TOut : class
    {
        Builder.Registrations.Remove(typeof(IEntityExportService<TOut, TVersion>));
        Builder.Registrations.Remove(typeof(IFileFieldAutoMapper<TVersion>));
        Builder.Registrations.Remove(typeof(IFileFieldWriteFilter<TOut, TVersion>));
        Builder.Registrations.Remove(typeof(IEntityExportMapper<TOut, TVersion, TOut>));
        Builder.Registrations.Remove(typeof(IFileFieldWriteFilterFactory<TVersion>));
    }

    public IoConfigurator<TBuilder, TKey, TEntity, TVersion> WithMapper<TOut, TMapper>()
        where TOut : class
        where TMapper : IEntityExportMapper<TEntity, TVersion, TOut>
    {
        Builder.WithRegistration<IEntityExportMapper<TEntity, TVersion, TOut>, TMapper>();

        RemoveExportEntityRegistrations<TEntity>();
        AddExportEntityRegistrations<TOut>();

        return this;
    }

    public IoConfigurator<TBuilder, TKey, TEntity, TVersion> WithMapper<TOut>(Func<TEntity, TOut> mapper)
        where TOut : class
    {
        Builder.WithRegistrationInstance(typeof(IEntityExportMapper<TEntity, TVersion, TOut>), new EntityExportMapper<TEntity, TVersion, TOut>(mapper));

        RemoveExportEntityRegistrations<TEntity>();
        AddExportEntityRegistrations<TOut>();

        return this;
    }
}
