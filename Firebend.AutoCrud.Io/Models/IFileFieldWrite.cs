using System;

namespace Firebend.AutoCrud.Io.Models;

public interface IFileFieldWrite<T> : IFileField
{
    public Func<T, object> Writer { get; }
}
