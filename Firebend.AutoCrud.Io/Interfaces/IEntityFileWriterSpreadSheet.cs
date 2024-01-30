using Firebend.AutoCrud.Core.Interfaces;

namespace Firebend.AutoCrud.Io.Interfaces;

public interface IEntityFileWriterSpreadSheet<TVersion> : IEntityFileWriter<TVersion>
    where TVersion : class, IAutoCrudApiVersion;
