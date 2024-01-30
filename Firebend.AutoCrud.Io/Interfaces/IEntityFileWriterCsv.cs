using Firebend.AutoCrud.Core.Interfaces;

namespace Firebend.AutoCrud.Io.Interfaces;

public interface IEntityFileWriterCsv<TVersion> : IEntityFileWriter<TVersion>
    where TVersion : class, IAutoCrudApiVersion;
