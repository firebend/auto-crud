using System;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IEntityFileWriterFactory : IDisposable
    {
        IEntityFileWriter Get(EntityFileType type);
    }
}
